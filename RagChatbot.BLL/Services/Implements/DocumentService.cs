using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using RagChatbot.BLL.Services.Interfaces;
using RagChatbot.DAL.Entities;
using RagChatbot.DAL.Repositories.Interfaces;

namespace RagChatbot.BLL.Services.Implements
{
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IServiceProvider _serviceProvider;

        public DocumentService(IDocumentRepository documentRepository, IServiceProvider serviceProvider)
        {
            _documentRepository = documentRepository;
            _serviceProvider = serviceProvider;
        }

        public IEnumerable<Document> GetDocumentsBySubject(Guid subjectId)
        {
            return _documentRepository.GetDocumentsBySubjectId(subjectId);
        }

        public Document GetDocumentById(Guid id)
        {
            return _documentRepository.GetById(id);
        }

        public async Task<bool> UploadDocumentAsync(Guid subjectId, string fileName, Stream fileStream, string uploadPath)
        {
            try
            {
                // 1. Kiểm tra và tạo thư mục nếu chưa tồn tại
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                // 2. Chống trùng tên file: Thêm mã GUID ngẫu nhiên vào trước tên gốc
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + fileName;
                string physicalFilePath = Path.Combine(uploadPath, uniqueFileName);

                // 3. Copy file vật lý xuống ổ cứng của Server
                using (var stream = new FileStream(physicalFilePath, FileMode.Create))
                {
                    await fileStream.CopyToAsync(stream);
                }

                // 4. Lưu thông tin vào Database
                var document = new Document
                {
                    Id = Guid.NewGuid(),
                    SubjectId = subjectId,
                    FileName = fileName, // Lưu tên gốc cho đẹp
                    FilePath = "/uploads/" + uniqueFileName, // Đường dẫn tương đối để hiển thị lên Web
                    Status = DocumentStatus.Pending, // Mặc định là Pending chờ Chatbot xử lý
                    UploadedAt = DateTime.UtcNow
                };

                _documentRepository.Add(document);

                // 5. Tự động chạy RAG pipeline (Chunking & Embedding) cho tài liệu mới
                // Tạo một Scope mới trong luồng ngầm để tránh lỗi Disposed DbContext khi request kết thúc
                var serviceProvider = _serviceProvider;
                _ = Task.Run(async () =>
                {
                    using (var scope = serviceProvider.CreateScope())
                    {
                        var scopedRagService = scope.ServiceProvider.GetRequiredService<IRagService>();
                        await scopedRagService.ProcessDocumentAsync(document.Id, physicalFilePath);
                    }
                });

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteDocument(Guid id, string rootPath)
        {
            var document = _documentRepository.GetById(id);
            if (document == null) return false;

            // 1. Xóa file vật lý trên ổ cứng trước
            // Chuyển "/uploads/ten-file.pdf" thành đường dẫn thật trên máy
            string physicalPath = Path.Combine(rootPath, document.FilePath.TrimStart('/'));
            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
            }

            // 2. Xóa dữ liệu trong DB
            _documentRepository.Delete(id);
            return true;
        }
    }
}
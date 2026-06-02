using RagChatbot.BLL.Helpers;
using RagChatbot.BLL.Services.Interfaces;
using RagChatbot.DAL.Data;
using RagChatbot.DAL.Entities;
using RagChatbot.DAL.Repositories.Interfaces;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig;
using Pgvector;

namespace RagChatbot.BLL.Services.Implements
{
    public class DocumentProcessingService : IDocumentProcessingService
    {
        private readonly IDocumentRepository _documentRepo;
        private readonly IAIService _aiService;
        private readonly ApplicationDbContext _context;

        public DocumentProcessingService(IDocumentRepository documentRepo, IAIService aiService, ApplicationDbContext context)
        {
            _documentRepo = documentRepo;
            _aiService = aiService;
            _context = context;
        }

        public async Task<bool> ProcessDocumentAsync(Guid documentId, string rootPath)
        {
            // 1. Lấy thông tin tài liệu từ DB
            var doc = _documentRepo.GetById(documentId);
            if (doc == null) return false;

            string physicalPath = Path.Combine(rootPath, doc.FilePath.TrimStart('/'));
            if (!File.Exists(physicalPath)) return false;

            // 2. Đọc toàn bộ nội dung chữ (Hỗ trợ TXT và PDF)
            string fullText = string.Empty;
            var extension = Path.GetExtension(doc.FileName).ToLower();

            try
            {
                if (extension == ".txt")
                {
                    fullText = await File.ReadAllTextAsync(physicalPath);
                }
                else if (extension == ".pdf")
                {
                    fullText = ExtractTextFromPdf(physicalPath);
                }
                else
                {
                    return false; // Định dạng chưa hỗ trợ
                }

                // 3. Băm nhỏ văn bản (Mỗi chunk khoảng 300 từ)
                var chunks = TextChunker.SplitText(fullText, chunkSize: 300, overlapSize: 50);

                // 4. Gọi AI chuyển chữ thành Vector số
                foreach (var chunk in chunks)
                {
                    float[] vectorArray = await _aiService.GenerateEmbeddingAsync(chunk);
                    var docChunk = new DocumentChunk
                    {
                        Id = Guid.NewGuid(),
                        DocumentId = documentId,
                        TextContent = chunk,
                        // Phép thuật của pgvector: Biến mảng float[] thành kiểu Vector
                        Embedding = new Vector(vectorArray)
                    };

                    _context.DocumentChunks.Add(docChunk); // Đưa vào hàng đợi lưu
                    doc.Status = DocumentStatus.Completed;
                }
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi tại RAG Pipeline: {ex.Message}", ex);
            }
        }

        // Hàm hỗ trợ chuyên móc nội dung từ PDF
        private string ExtractTextFromPdf(string filePath)
        {
            StringBuilder textBuilder = new StringBuilder();

            // Dùng PdfPig mở file
            using (PdfDocument document = PdfDocument.Open(filePath))
            {
                // Lặp qua từng trang để gom chữ
                foreach (var page in document.GetPages())
                {
                    textBuilder.AppendLine(page.Text);
                }
            }
            return textBuilder.ToString();
        }
    }
}
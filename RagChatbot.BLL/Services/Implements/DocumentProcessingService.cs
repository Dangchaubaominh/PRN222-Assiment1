using RagChatbot.BLL.Helpers;
using RagChatbot.BLL.Services.Interfaces;
using RagChatbot.DAL.Data;
using RagChatbot.DAL.Entities;
using RagChatbot.DAL.Repositories.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig;
using Pgvector;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

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
                else if (extension == ".docx" || extension == ".doc")
                {
                    fullText = ExtractTextFromDocx(physicalPath);
                }
                else
                {
                    return false; // Định dạng chưa hỗ trợ
                }

                // 3. Semantic Chunking: chia văn bản theo ranh giới đoạn văn / câu
                //    (không bao giờ cắt giữa câu, overlap theo câu thay vì từ)
                var chunks = SemanticChunker.SplitText(fullText,
                                                       maxWordsPerChunk: 400,
                                                       overlapSentences:  2);

                // 4. Gọi AI chuyển từng chunk thành Vector embedding 768 chiều
                doc.Status = DocumentStatus.Processing;
                await _context.SaveChangesAsync();

                foreach (var chunk in chunks)
                {
                    float[] vectorArray = await _aiService.GenerateEmbeddingAsync(chunk);
                    var docChunk = new DocumentChunk
                    {
                        Id          = Guid.NewGuid(),
                        DocumentId  = documentId,
                        TextContent = chunk,
                        Embedding   = new Vector(vectorArray)
                    };
                    _context.DocumentChunks.Add(docChunk);
                }

                doc.Status = DocumentStatus.Completed;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi tại RAG Pipeline: {ex.Message}", ex);
            }
        }

        private string ExtractTextFromPdf(string filePath)
        {
            var sb = new StringBuilder();
            using (PdfDocument document = PdfDocument.Open(filePath))
            {
                foreach (var page in document.GetPages())
                    sb.AppendLine(page.Text);
            }
            return sb.ToString();
        }

        private string ExtractTextFromDocx(string filePath)
        {
            var sb = new StringBuilder();
            using var wordDoc = WordprocessingDocument.Open(filePath, isEditable: false);
            var body = wordDoc.MainDocumentPart?.Document?.Body;
            if (body == null) return string.Empty;

            foreach (var para in body.Descendants<Paragraph>())
            {
                string line = para.InnerText.Trim();
                if (line.Length > 0)
                    sb.AppendLine(line);
            }
            return sb.ToString();
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using RagChatbot.DAL.Entities;

namespace RagChatbot.BLL.Services.Interfaces
{
    public interface IDocumentService
    {
        IEnumerable<Document> GetDocumentsBySubject(Guid subjectId);
        Document GetDocumentById(Guid id);

        // Dùng Task để xử lý bất đồng bộ (Async) khi copy file lớn
        Task<bool> UploadDocumentAsync(Guid subjectId, string fileName, Stream fileStream, string uploadPath);

        bool DeleteDocument(Guid id, string rootPath);
    }
}
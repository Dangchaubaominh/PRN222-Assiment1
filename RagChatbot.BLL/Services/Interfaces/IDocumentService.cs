using RagChatbot.BLL.DTOs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RagChatbot.BLL.Services.Interfaces
{
    public interface IDocumentService
    {
        IEnumerable<DocumentDto> GetDocumentsBySubject(Guid subjectId);
        DocumentDto GetDocumentById(Guid id);
        Task<bool> UploadDocumentAsync(Guid subjectId, string fileName, Stream fileStream, string uploadPath);
        bool DeleteDocument(Guid id, string rootPath);
    }
}

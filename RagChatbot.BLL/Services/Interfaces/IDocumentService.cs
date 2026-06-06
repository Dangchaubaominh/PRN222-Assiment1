using RagChatbot.BLL.DTOs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RagChatbot.BLL.Services.Interfaces
{
    public enum DocumentUploadResult
    {
        Success,
        Duplicate,
        Error
    }

    public interface IDocumentService
    {
        IEnumerable<DocumentDto> GetDocumentsBySubject(Guid subjectId);
        DocumentDto GetDocumentById(Guid id);
        Task<DocumentUploadResult> UploadDocumentAsync(Guid subjectId, string fileName, Stream fileStream, string uploadPath);
        bool DeleteDocument(Guid id, string rootPath);

        // Lấy danh sách các chunk đã được chia từ tài liệu
        IEnumerable<DocumentChunkDto> GetChunksByDocumentId(Guid documentId);
    }
}

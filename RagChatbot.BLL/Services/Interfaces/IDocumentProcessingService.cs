using System;
using System.Threading.Tasks;

namespace RagChatbot.BLL.Services.Interfaces
{
    public interface IDocumentProcessingService
    {
        Task<bool> ProcessDocumentAsync(Guid documentId, string rootPath);
    }
}
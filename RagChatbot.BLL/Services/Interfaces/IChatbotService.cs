using System;
using System.Threading.Tasks;

namespace RagChatbot.BLL.Services.Interfaces
{
    public interface IChatbotService
    {
        Task<string> GetAnswerAsync(Guid subjectId, string userMessage);
    }
}
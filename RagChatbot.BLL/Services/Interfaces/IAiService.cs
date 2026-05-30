using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RagChatbot.DAL.Entities;

namespace RagChatbot.BLL.Services.Interfaces
{
    public interface IAiService
    {
        Task<(string Answer, List<string> Citations)> GenerateResponseAsync(Guid subjectId, string userMessage, List<ChatMessage> conversationHistory, string embeddingModel = "multilingual-e5-base");
    }
}

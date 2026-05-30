using System;
using System.Collections.Generic;
using RagChatbot.DAL.Entities;

namespace RagChatbot.DAL.Repositories.Interfaces
{
    public interface IChatRepository
    {
        IEnumerable<ChatSession> GetSessionsByAccountAndSubject(Guid accountId, Guid subjectId);
        ChatSession? GetSessionById(Guid sessionId);
        void AddSession(ChatSession session);
        void DeleteSession(Guid sessionId);
        void AddMessage(ChatMessage message);
        IEnumerable<ChatMessage> GetMessagesBySessionId(Guid sessionId);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RagChatbot.DAL.Data;
using RagChatbot.DAL.Entities;
using RagChatbot.DAL.Repositories.Interfaces;

namespace RagChatbot.DAL.Repositories.Implements
{
    public class ChatRepository : IChatRepository
    {
        private readonly ApplicationDbContext _context;

        public ChatRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<ChatSession> GetSessionsByAccountAndSubject(Guid accountId, Guid subjectId)
        {
            return _context.ChatSessions
                           .Where(s => s.AccountId == accountId && s.SubjectId == subjectId)
                           .OrderByDescending(s => s.CreatedAt)
                           .ToList();
        }

        public ChatSession? GetSessionById(Guid sessionId)
        {
            return _context.ChatSessions
                           .Include(s => s.Messages)
                           .FirstOrDefault(s => s.Id == sessionId);
        }

        public void AddSession(ChatSession session)
        {
            _context.ChatSessions.Add(session);
            _context.SaveChanges();
        }

        public void DeleteSession(Guid sessionId)
        {
            var session = _context.ChatSessions.Find(sessionId);
            if (session != null)
            {
                _context.ChatSessions.Remove(session);
                _context.SaveChanges();
            }
        }

        public void AddMessage(ChatMessage message)
        {
            _context.ChatMessages.Add(message);
            _context.SaveChanges();
        }

        public IEnumerable<ChatMessage> GetMessagesBySessionId(Guid sessionId)
        {
            return _context.ChatMessages
                           .Where(m => m.SessionId == sessionId)
                           .OrderBy(m => m.CreatedAt)
                           .ToList();
        }
    }
}

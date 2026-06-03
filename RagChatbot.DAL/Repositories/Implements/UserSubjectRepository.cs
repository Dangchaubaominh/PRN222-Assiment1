using RagChatbot.DAL.Data;
using RagChatbot.DAL.Entities;
using RagChatbot.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RagChatbot.DAL.Repositories.Implements
{
    public class UserSubjectRepository : IUserSubjectRepository
    {
        private readonly ApplicationDbContext _context;

        public UserSubjectRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Subject> GetSubjectsByUserId(int userId)
        {
            return _context.UserSubjects
                .Where(us => us.UserId == userId)
                .Include(us => us.Subject)
                .Select(us => us.Subject)
                .OrderBy(s => s.CreatedAt)
                .ToList();
        }

        public IEnumerable<User> GetUsersBySubjectId(Guid subjectId)
        {
            return _context.UserSubjects
                .Where(us => us.SubjectId == subjectId)
                .Include(us => us.User)
                .Select(us => us.User)
                .OrderBy(u => u.Role)
                .ThenBy(u => u.Username)
                .ToList();
        }

        public bool IsAssigned(int userId, Guid subjectId)
        {
            return _context.UserSubjects.Any(us => us.UserId == userId && us.SubjectId == subjectId);
        }

        public void Assign(int userId, Guid subjectId)
        {
            if (!IsAssigned(userId, subjectId))
            {
                _context.UserSubjects.Add(new UserSubject { UserId = userId, SubjectId = subjectId });
                _context.SaveChanges();
            }
        }

        public void Remove(int userId, Guid subjectId)
        {
            var entry = _context.UserSubjects
                .FirstOrDefault(us => us.UserId == userId && us.SubjectId == subjectId);
            if (entry != null)
            {
                _context.UserSubjects.Remove(entry);
                _context.SaveChanges();
            }
        }
    }
}

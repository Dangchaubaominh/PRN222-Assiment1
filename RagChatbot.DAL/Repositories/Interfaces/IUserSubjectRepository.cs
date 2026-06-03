using RagChatbot.DAL.Entities;
using System;
using System.Collections.Generic;

namespace RagChatbot.DAL.Repositories.Interfaces
{
    public interface IUserSubjectRepository
    {
        IEnumerable<Subject> GetSubjectsByUserId(int userId);
        IEnumerable<User> GetUsersBySubjectId(Guid subjectId);
        bool IsAssigned(int userId, Guid subjectId);
        void Assign(int userId, Guid subjectId);
        void Remove(int userId, Guid subjectId);
    }
}

using RagChatbot.BLL.DTOs;
using System;
using System.Collections.Generic;

namespace RagChatbot.BLL.Services.Interfaces
{
    public interface IUserSubjectService
    {
        IEnumerable<SubjectDto> GetAssignedSubjects(int userId);
        IEnumerable<UserManageDto> GetAssignedUsers(Guid subjectId);
        IEnumerable<UserManageDto> GetAddableUsers(Guid subjectId, string requesterRole);
        void Assign(int userId, Guid subjectId);
        void Remove(int userId, Guid subjectId);
    }
}

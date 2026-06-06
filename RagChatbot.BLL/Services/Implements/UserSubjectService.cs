using RagChatbot.BLL.DTOs;
using RagChatbot.BLL.Services.Interfaces;
using RagChatbot.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RagChatbot.BLL.Services.Implements
{
    public class UserSubjectService : IUserSubjectService
    {
        private readonly IUserSubjectRepository _userSubjectRepository;
        private readonly IUserRepository _userRepository;

        public UserSubjectService(IUserSubjectRepository userSubjectRepository, IUserRepository userRepository)
        {
            _userSubjectRepository = userSubjectRepository;
            _userRepository = userRepository;
        }

        public IEnumerable<SubjectDto> GetAssignedSubjects(int userId)
        {
            return _userSubjectRepository.GetSubjectsByUserId(userId).Select(s => new SubjectDto
            {
                Id = s.Id,
                Code = s.Code,
                Name = s.Name,
                CreatedAt = s.CreatedAt
            });
        }

        public IEnumerable<UserManageDto> GetAssignedUsers(Guid subjectId)
        {
            return _userSubjectRepository.GetUsersBySubjectId(subjectId).Select(u => new UserManageDto
            {
                Id = u.Id,
                Username = u.Username,
                FullName = u.FullName,
                Role = u.Role
            });
        }

        // Lấy danh sách user chưa được gán vào môn học, lọc theo quyền người yêu cầu:
        // Admin: tất cả user chưa được gán (trừ Admin khác)
        // Lecturer: chỉ Student chưa được gán
        public IEnumerable<UserManageDto> GetAddableUsers(Guid subjectId, string requesterRole)
        {
            var assignedIds = _userSubjectRepository.GetUsersBySubjectId(subjectId)
                .Select(u => u.Id).ToHashSet();

            var allUsers = _userRepository.GetAll()
                .Where(u => !assignedIds.Contains(u.Id) && u.Role != "Admin");

            if (requesterRole == "Lecturer")
                allUsers = allUsers.Where(u => u.Role == "Student");

            return allUsers.Select(u => new UserManageDto
            {
                Id = u.Id,
                Username = u.Username,
                FullName = u.FullName,
                Role = u.Role
            });
        }

        public const int MaxTeachersPerSubject = 3;

        public int CountTeachersInSubject(Guid subjectId)
        {
            return _userSubjectRepository.GetUsersBySubjectId(subjectId)
                .Count(u => u.Role == "Lecturer" || u.Role == "Admin");
        }

        public AssignResult Assign(int userId, Guid subjectId)
        {
            var user = _userRepository.GetAll().FirstOrDefault(u => u.Id == userId);
            if (user != null && (user.Role == "Lecturer" || user.Role == "Admin"))
            {
                if (CountTeachersInSubject(subjectId) >= MaxTeachersPerSubject)
                    return AssignResult.TeacherLimitReached;
            }

            _userSubjectRepository.Assign(userId, subjectId);
            return AssignResult.Success;
        }

        public void Remove(int userId, Guid subjectId)
        {
            _userSubjectRepository.Remove(userId, subjectId);
        }
    }
}

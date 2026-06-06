using RagChatbot.BLL.DTOs;
using RagChatbot.BLL.Services.Interfaces;
using RagChatbot.DAL.Entities;
using RagChatbot.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RagChatbot.BLL.Services.Implements
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public UserDto Authenticate(string username, string password)
        {
            var userEntity = _userRepository.GetUserByCredentials(username, password);
            if (userEntity == null) return null;

            return new UserDto
            {
                Id = userEntity.Id,
                Username = userEntity.Username,
                FullName = userEntity.FullName,
                Role = userEntity.Role
            };
        }

        public IEnumerable<UserManageDto> GetAllUsers()
        {
            return _userRepository.GetAll().Select(ToDto);
        }

        public UserManageDto GetByUsername(string username)
        {
            var u = _userRepository.GetByUsername(username);
            return u == null ? null : ToDto(u);
        }

        public UserEditDto GetEditById(int id)
        {
            var u = _userRepository.GetById(id);
            if (u == null) return null;
            return new UserEditDto
            {
                Id       = u.Id,
                Username = u.Username,
                FullName = u.FullName,
                Email    = u.Email,
                Role     = u.Role
            };
        }

        public bool UpdateUserInfo(UserEditDto dto)
        {
            var entity = _userRepository.GetById(dto.Id);
            if (entity == null) return false;

            entity.FullName = dto.FullName;
            entity.Email    = dto.Email;
            entity.Role     = dto.Role;
            _userRepository.Update(entity);
            return true;
        }

        public string GeneratePasswordResetToken(string email)
        {
            var user = _userRepository.GetByEmail(email);
            if (user == null) return null;

            string token  = Guid.NewGuid().ToString("N"); // 32 ký tự hex
            DateTime expiry = DateTime.UtcNow.AddMinutes(30);
            _userRepository.SetResetToken(user.Id, token, expiry);
            return token;
        }

        public bool IsValidResetToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return false;
            var user = _userRepository.GetByResetToken(token);
            return user != null && user.PasswordResetExpiry > DateTime.UtcNow;
        }

        public bool ResetPassword(string token, string newPassword)
        {
            if (!IsValidResetToken(token)) return false;
            var user = _userRepository.GetByResetToken(token);
            _userRepository.UpdatePassword(user.Id, newPassword);
            return true;
        }

        private static UserManageDto ToDto(DAL.Entities.User u) => new UserManageDto
        {
            Id       = u.Id,
            Username = u.Username,
            FullName = u.FullName,
            Role     = u.Role,
            Email    = u.Email
        };

        public bool CreateUser(UserManageDto dto)
        {
            var entity = new User
            {
                Username = dto.Username,
                Password = dto.Password,
                Role     = dto.Role,
                FullName = dto.FullName,
                Email    = dto.Email
            };
            _userRepository.Add(entity);
            return true;
        }

        public bool DeleteUser(int id, string currentUsername)
        {
            var user = _userRepository.GetById(id);
            if (user == null) return false;

            // Admin không thể tự xóa chính mình
            if (user.Username == currentUsername) return false;

            _userRepository.Delete(id);
            return true;
        }
    }
}

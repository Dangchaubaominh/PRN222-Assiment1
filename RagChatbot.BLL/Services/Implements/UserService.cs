using RagChatbot.BLL.DTOs;
using RagChatbot.BLL.Services.Interfaces;
using RagChatbot.DAL.Entities;
using RagChatbot.DAL.Repositories.Interfaces;
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
            return _userRepository.GetAll().Select(u => new UserManageDto
            {
                Id = u.Id,
                Username = u.Username,
                FullName = u.FullName,
                Role = u.Role
            });
        }

        public bool CreateUser(UserManageDto dto)
        {
            var entity = new User
            {
                Username = dto.Username,
                Password = dto.Password,
                Role = dto.Role,
                FullName = dto.FullName
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

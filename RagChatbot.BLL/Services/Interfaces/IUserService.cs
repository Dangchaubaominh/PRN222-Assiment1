using RagChatbot.BLL.DTOs;
using System.Collections.Generic;

namespace RagChatbot.BLL.Services.Interfaces
{
    public interface IUserService
    {
        UserDto Authenticate(string username, string password);
        IEnumerable<UserManageDto> GetAllUsers();
        bool CreateUser(UserManageDto dto);
        bool DeleteUser(int id, string currentUsername);
    }
}

using RagChatbot.BLL.DTOs;
using System.Collections.Generic;

namespace RagChatbot.BLL.Services.Interfaces
{
    public interface IUserService
    {
        UserDto Authenticate(string username, string password);
        IEnumerable<UserManageDto> GetAllUsers();
        UserManageDto GetByUsername(string username);
        UserEditDto GetEditById(int id);
        bool CreateUser(UserManageDto dto);
        bool UpdateUserInfo(UserEditDto dto);
        bool DeleteUser(int id, string currentUsername);

        // Quên mật khẩu
        string GeneratePasswordResetToken(string email);  // null nếu email không tồn tại
        bool IsValidResetToken(string token);
        bool ResetPassword(string token, string newPassword);
    }
}

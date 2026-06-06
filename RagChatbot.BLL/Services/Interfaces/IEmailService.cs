using System.Threading.Tasks;

namespace RagChatbot.BLL.Services.Interfaces
{
    public interface IEmailService
    {
        /// <summary>
        /// Gửi thông tin tài khoản đến email người dùng sau khi admin tạo.
        /// </summary>
        /// <param name="adminName">Tên admin gửi (hiển thị trong email)</param>
        /// <param name="adminEmail">Gmail của admin (dùng làm Reply-To)</param>
        Task<bool> SendAccountCredentialsAsync(string toEmail, string fullName,
                                               string username, string password,
                                               string adminName, string adminEmail);

        Task<bool> SendPasswordResetEmailAsync(string toEmail, string fullName, string resetLink);
    }
}

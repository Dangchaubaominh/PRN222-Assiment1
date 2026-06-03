using System.ComponentModel.DataAnnotations;

namespace RagChatbot.BLL.DTOs
{
    public class UserManageDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [MaxLength(50)]
        public string Username { get; set; }

        [MaxLength(255)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn vai trò")]
        public string Role { get; set; }

        [MaxLength(100)]
        public string FullName { get; set; }
    }
}

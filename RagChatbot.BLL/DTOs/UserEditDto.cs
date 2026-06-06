using System.ComponentModel.DataAnnotations;

namespace RagChatbot.BLL.DTOs
{
    public class UserEditDto
    {
        public int Id { get; set; }

        // Chỉ hiển thị, không cho chỉnh sửa
        public string Username { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Gmail không được để trống")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
        [MaxLength(150)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn vai trò")]
        public string Role { get; set; }
    }
}

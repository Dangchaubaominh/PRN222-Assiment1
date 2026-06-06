using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RagChatbot.DAL.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required]
        [MaxLength(255)]
        public string Password { get; set; }

        [Required]
        [MaxLength(20)]
        public string Role { get; set; }

        [MaxLength(100)]
        public string FullName { get; set; }

        [MaxLength(150)]
        public string? Email { get; set; }

        // Dùng cho tính năng quên mật khẩu
        [MaxLength(64)]
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetExpiry { get; set; }

        public virtual ICollection<UserSubject> UserSubjects { get; set; }
    }
}
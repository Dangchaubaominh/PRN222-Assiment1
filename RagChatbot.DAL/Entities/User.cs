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

        public virtual ICollection<UserSubject> UserSubjects { get; set; }
    }
}
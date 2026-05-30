using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RagChatbot.DAL.Entities
{
    [Table("ChatSessions")]
    public class ChatSession
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid SubjectId { get; set; }

        public Guid AccountId { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; } = "Hội thoại mới";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("SubjectId")]
        public virtual Subject? Subject { get; set; }

        [ForeignKey("AccountId")]
        public virtual Account? Account { get; set; }

        public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}

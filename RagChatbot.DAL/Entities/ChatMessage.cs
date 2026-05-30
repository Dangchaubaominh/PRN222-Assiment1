using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RagChatbot.DAL.Entities
{
    [Table("ChatMessages")]
    public class ChatMessage
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid SessionId { get; set; }

        [Required]
        [StringLength(50)]
        public string Sender { get; set; } = string.Empty; // "User" or "Bot"

        [Required]
        public string Content { get; set; } = string.Empty;

        public string? CitationsJson { get; set; } // JSON list of text chunks used as references

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("SessionId")]
        public virtual ChatSession? Session { get; set; }
    }
}

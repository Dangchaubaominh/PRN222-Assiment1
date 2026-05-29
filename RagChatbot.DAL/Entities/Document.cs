using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RagChatbot.DAL.Entities
{
    [Table("Documents")]
    public class Document
    {
        [Key]
        [Column(TypeName = "uuid")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column(TypeName = "uuid")]
        public Guid SubjectId { get; set; }

        [Required]
        [StringLength(500)]
        [Column(TypeName = "varchar(500)")]
        public string FileName { get; set; }

        [Required]
        [StringLength(1000)]
        [Column(TypeName = "varchar(1000)")]
        public string FilePath { get; set; }

        public DocumentStatus Status { get; set; } = DocumentStatus.Pending;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("SubjectId")]
        public virtual Subject Subject { get; set; }
    }
}

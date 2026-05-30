using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RagChatbot.DAL.Entities
{
    [Table("DocumentChunks")]
    public class DocumentChunk
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid DocumentId { get; set; }

        public int ChunkIndex { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        public string VectorJson { get; set; } = "[]"; // Serialized float[] vector for cosine similarity

        public int ChunkSize { get; set; }

        [Required]
        [StringLength(100)]
        public string ChunkingStrategy { get; set; } = string.Empty;

        [ForeignKey("DocumentId")]
        public virtual Document? Document { get; set; }
    }
}

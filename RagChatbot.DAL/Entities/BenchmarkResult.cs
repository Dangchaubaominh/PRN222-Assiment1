using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RagChatbot.DAL.Entities
{
    [Table("BenchmarkResults")]
    public class BenchmarkResult
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTime RunAt { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(100)]
        public string EmbeddingModel { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ChunkingStrategy { get; set; } = string.Empty;

        public double LatencyMs { get; set; }

        public double Faithfulness { get; set; } // 0.0 to 1.0

        public double AnswerRelevance { get; set; } // 0.0 to 1.0

        public double ContextRecall { get; set; } // 0.0 to 1.0

        public double Accuracy { get; set; } // 0.0 to 1.0

        public bool IsRag { get; set; }
    }
}

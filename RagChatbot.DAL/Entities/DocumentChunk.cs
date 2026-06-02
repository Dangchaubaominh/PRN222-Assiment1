using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pgvector;

namespace RagChatbot.DAL.Entities
{
    public class DocumentChunk
    {
        [Key]
        public Guid Id { get; set; }

        // Liên kết với bảng Document gốc
        public Guid DocumentId { get; set; }
        [ForeignKey("DocumentId")]
        public Document Document { get; set; }

        public string TextContent { get; set; } // Chứa đoạn văn bản ngắn (khoảng 300 chữ)

        // ĐÂY LÀ PHÉP THUẬT: Kiểu dữ liệu Vector đặc biệt của pgvector
        [Column(TypeName = "vector(768)")]
        public Vector Embedding { get; set; }
    }
}
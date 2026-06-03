using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace RagChatbot.DAL.Entities
{
    [Table("Subjects")]
    public class Subject
    {
        [Key]
        [Column(TypeName = "uuid")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "Mã môn học không được để trống.")]
        [StringLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Tên môn học không được để trống.")]
        [StringLength(255)]
        [Column(TypeName = "varchar(255)")]
        public string Name { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Document> Documents { get; set; }
        public virtual ICollection<UserSubject> UserSubjects { get; set; }
    }
}

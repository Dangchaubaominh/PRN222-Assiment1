using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace RagChatbot.DAL.Entities
{
    [Table("UserSubjects")]
    public class UserSubject
    {
        public int UserId { get; set; }
        public virtual User User { get; set; }

        public Guid SubjectId { get; set; }
        public virtual Subject Subject { get; set; }
    }
}

using System;
using System.ComponentModel.DataAnnotations;

namespace RagChatbot.BLL.DTOs
{
    public class SubjectDto
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Tên môn học không được để trống")]
        public string Name { get; set; }

        public string Code { get; set; }

        // ĐÃ SỬA: Đổi từ string sang DateTime
        public DateTime CreatedAt { get; set; }
    }
}
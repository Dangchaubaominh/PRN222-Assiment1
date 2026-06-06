using System;

namespace RagChatbot.BLL.DTOs
{
    public class DocumentChunkDto
    {
        public Guid Id { get; set; }
        public int Index { get; set; }        // Số thứ tự chunk (bắt đầu từ 1)
        public string TextContent { get; set; }
        public int WordCount { get; set; }
    }
}

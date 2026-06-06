using System;
using System.Collections.Generic;
using System.Linq;

namespace RagChatbot.BLL.Helpers
{
    /// <summary>
    /// [Deprecated] Chunker cắt cứng theo số từ — không tôn trọng ranh giới câu.
    /// Hệ thống hiện dùng <see cref="SemanticChunker"/> thay thế.
    /// </summary>
    [Obsolete("Dùng SemanticChunker thay thế. TextChunker cắt cứng theo từ, không tôn trọng ranh giới câu.")]
    public static class TextChunker
    {
        public static List<string> SplitText(string text, int chunkSize = 300, int overlapSize = 50)
        {
            if (string.IsNullOrWhiteSpace(text)) return new List<string>();

            var words = text.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            var chunks = new List<string>();
            int i = 0;

            while (i < words.Length)
            {
                var currentChunkWords = words.Skip(i).Take(chunkSize);
                chunks.Add(string.Join(" ", currentChunkWords));

                i += (chunkSize - overlapSize);
                if (chunkSize <= overlapSize) break;
            }

            return chunks;
        }
    }
}
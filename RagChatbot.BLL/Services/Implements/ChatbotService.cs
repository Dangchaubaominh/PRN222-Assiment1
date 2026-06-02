using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;
using RagChatbot.BLL.Services.Interfaces;
using RagChatbot.DAL.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RagChatbot.BLL.Services.Implements
{
    public class ChatbotService : IChatbotService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAIService _aiService;

        public ChatbotService(ApplicationDbContext context, IAIService aiService)
        {
            _context = context;
            _aiService = aiService;
        }

        public async Task<string> GetAnswerAsync(Guid subjectId, string userMessage)
        {
            try
            {
                // 1. Nhúng câu hỏi thành Vector 768 chiều
                float[] questionVectorArray = await _aiService.GenerateEmbeddingAsync(userMessage);
                var queryVector = new Pgvector.Vector(questionVectorArray);

                // 2. Truy vấn Database lấy 3 đoạn tài liệu khớp nhất
                var similarChunks = await _context.DocumentChunks
                    .Include(c => c.Document)
                    .Where(c => c.Document.SubjectId == subjectId)
                    .OrderBy(c => c.Embedding.CosineDistance(queryVector))
                    .Take(3)
                    .Select(c => c.TextContent)
                    .ToListAsync();

                if (!similarChunks.Any())
                {
                    return "Môn học này hiện chưa có tài liệu nào. Vui lòng upload tài liệu trước khi hỏi.";
                }

                // 3. Ghép tài liệu thành Context
                string contextText = string.Join("\n\n---\n\n", similarChunks);

                // 4. Tạo Prompt và gọi AI
                string finalPrompt = $"TÀI LIỆU CUNG CẤP:\n{contextText}\n\nCÂU HỎI CỦA NGƯỜI DÙNG:\n{userMessage}";
                return await _aiService.GenerateChatResponseAsync(finalPrompt);
            }
            catch (Exception ex)
            {
                return $"Hệ thống gặp lỗi nội bộ: {ex.Message}";
            }
        }
    }
}
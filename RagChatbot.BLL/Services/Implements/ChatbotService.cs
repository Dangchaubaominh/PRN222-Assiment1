using RagChatbot.BLL.Services.Interfaces;
using RagChatbot.DAL.Repositories.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RagChatbot.BLL.Services.Implements
{
    public class ChatbotService : IChatbotService
    {
        private readonly IDocumentChunkRepository _chunkRepository;
        private readonly IAIService _aiService;

        public ChatbotService(IDocumentChunkRepository chunkRepository, IAIService aiService)
        {
            _chunkRepository = chunkRepository;
            _aiService = aiService;
        }

        public async Task<string> GetAnswerAsync(Guid subjectId, string userMessage)
        {
            try
            {
                // 1. Nhúng câu hỏi thành Vector 768 chiều
                float[] questionVector = await _aiService.GenerateEmbeddingAsync(userMessage);

                // 2. Tìm 3 đoạn văn bản gần nhất trong phạm vi môn học
                var similarChunks = (await _chunkRepository.SearchSimilarChunksAsync(subjectId, questionVector, topK: 3))
                    .ToList();

                if (!similarChunks.Any())
                    return "Môn học này hiện chưa có tài liệu nào. Vui lòng upload tài liệu trước khi hỏi.";

                // 3. Ghép context và gọi AI
                string contextText = string.Join("\n\n---\n\n", similarChunks);
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

using System.Threading.Tasks;

namespace RagChatbot.BLL.Services.Interfaces
{
    public interface IAIService
    {
        // Hàm này nhận vào 1 đoạn văn và trả về 1 mảng số (Vector)
        Task<float[]> GenerateEmbeddingAsync(string text);

        Task<string> GenerateChatResponseAsync(string prompt);
    }
}
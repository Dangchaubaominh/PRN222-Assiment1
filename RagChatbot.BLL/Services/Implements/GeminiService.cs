using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RagChatbot.BLL.Services.Interfaces;

namespace RagChatbot.BLL.Services.Implements
{
    public class GeminiService : IAIService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly string _apiKey;

        public GeminiService(IConfiguration config)
        {
            _apiKey = config["Gemini:ApiKey"];
        }

        public async Task<float[]> GenerateEmbeddingAsync(string text)
        {
            // 1. Đổi sang Model nhúng mới nhất: gemini-embedding-001
            string url = $"https://generativelanguage.googleapis.com/v1/models/gemini-embedding-001:embedContent?key={_apiKey}";

            var requestBody = new
            {
                model = "models/gemini-embedding-001",
                content = new
                {
                    parts = new[] { new { text = text } }
                },
                // 2. CỰC KỲ QUAN TRỌNG: ÉP Google trả về đúng 768 chiều 
                // để khớp tuyệt đối với cấu hình PostgreSQL của bạn
                outputDimensionality = 768
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, jsonContent);

            // 3. Xử lý báo lỗi chi tiết (bắt tận tay thông báo lỗi từ server Google)
            if (!response.IsSuccessStatusCode)
            {
                string errorDetail = await response.Content.ReadAsStringAsync();
                throw new System.Exception($"Lỗi từ Gemini API (HTTP {(int)response.StatusCode}): {errorDetail}");
            }

            var responseString = await response.Content.ReadAsStringAsync();
            using var jsonDoc = JsonDocument.Parse(responseString);

            var values = jsonDoc.RootElement
                .GetProperty("embedding")
                .GetProperty("values")
                .EnumerateArray();

            var vectorList = new System.Collections.Generic.List<float>();
            foreach (var value in values)
            {
                vectorList.Add(value.GetSingle());
            }

            return vectorList.ToArray();
        }
        public async Task<string> GenerateChatResponseAsync(string prompt)
        {
            string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";

            // Gắn System Prompt thẳng vào câu hỏi để ép AI không được "bịa" thông tin
            string fullPrompt = @"
            Bạn là trợ lý học tập AI.

            Trước khi trả lời, hãy xác định loại câu hỏi:

            - Nếu là câu giao tiếp thông thường (xin chào, cảm ơn, tạm biệt, giới thiệu bản thân, hỏi ngày tháng, thời gian, thời tiết...) thì trả lời bình thường.

            - Nếu là câu hỏi học tập hoặc liên quan đến nội dung môn học thì chỉ sử dụng thông tin trong phần NGỮ CẢNH bên dưới.

            Đối với câu hỏi học tập:
            - Không được sử dụng kiến thức bên ngoài.
            - Không được suy đoán.
            - Không được bịa thông tin.
            - Nếu ngữ cảnh không chứa câu trả lời thì trả lời:
              'Xin lỗi, tôi không tìm thấy thông tin này trong tài liệu môn học.'

            NGỮ CẢNH:
            " + prompt;

            var requestBody = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = fullPrompt } } }
                }
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, jsonContent);

            if (!response.IsSuccessStatusCode)
            {
                string errorDetail = await response.Content.ReadAsStringAsync();
                throw new System.Exception($"Lỗi Gemini Chat API: {errorDetail}");
            }

            var responseString = await response.Content.ReadAsStringAsync();
            using var jsonDoc = JsonDocument.Parse(responseString);

            // Bóc tách câu trả lời từ JSON trả về của Gemini
            string answer = jsonDoc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? "Không có câu trả lời từ AI";

            return answer;
        }
    }
}
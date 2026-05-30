using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RagChatbot.BLL.Services.Interfaces;
using RagChatbot.DAL.Entities;

namespace RagChatbot.BLL.Services.Implements
{
    public class AiService : IAiService
    {
        private readonly IRagService _ragService;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public AiService(IRagService ragService, IConfiguration configuration)
        {
            _ragService = ragService;
            _configuration = configuration;
            _httpClient = new HttpClient();
        }

        public async Task<(string Answer, List<string> Citations)> GenerateResponseAsync(
            Guid subjectId, 
            string userMessage, 
            List<ChatMessage> conversationHistory, 
            string embeddingModel = "multilingual-e5-base")
        {
            // 1. Retrieve the top 3 relevant chunks from the documents under this subject
            var relevantChunks = _ragService.SearchRelevantChunks(subjectId, userMessage, embeddingModel, topK: 3).ToList();

            if (!relevantChunks.Any())
            {
                return (
                    "Tôi xin lỗi, tôi không tìm thấy tài liệu nào liên quan đến câu hỏi của bạn. Vui lòng tải lên tài liệu môn học phù hợp trước khi đặt câu hỏi.",
                    new List<string>()
                );
            }

            // Extract unique citations/source documents
            var citations = relevantChunks
                .Select(c => c.Document != null ? c.Document.FileName : "Tài liệu không tên")
                .Distinct()
                .ToList();

            // Construct references list for RAG scope verification
            var contextBuilder = new StringBuilder();
            foreach (var chunk in relevantChunks)
            {
                string source = chunk.Document != null ? chunk.Document.FileName : "Tài liệu";
                contextBuilder.AppendLine($"[{source}]: {chunk.Content}");
            }

            // Check if API key is provided for Gemini or OpenAI in appsettings
            string? geminiKey = _configuration["Gemini:ApiKey"];
            string? openAiKey = _configuration["OpenAI:ApiKey"];

            if (!string.IsNullOrEmpty(geminiKey))
            {
                try
                {
                    string answer = await CallGeminiApi(userMessage, contextBuilder.ToString(), conversationHistory, geminiKey);
                    return (answer, citations);
                }
                catch (Exception)
                {
                    // Fall back to offline generation if API fails
                }
            }
            else if (!string.IsNullOrEmpty(openAiKey))
            {
                try
                {
                    string answer = await CallOpenAiApi(userMessage, contextBuilder.ToString(), conversationHistory, openAiKey);
                    return (answer, citations);
                }
                catch (Exception)
                {
                    // Fall back to offline generation if API fails
                }
            }

            // OFFLINE HIGH-FIDELITY RAG RESPONSE COMPILER (Fallback & local mode)
            string localAnswer = CompileOfflineResponse(userMessage, relevantChunks);
            return (localAnswer, citations);
        }

        private async Task<string> CallGeminiApi(string query, string context, List<ChatMessage> history, string apiKey)
        {
            string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}";
            
            var systemInstruction = "Bạn là trợ lý học tập thông minh. Hãy trả lời câu hỏi dựa TRÊN VÀ CHỈ DỰA TRÊN ngữ cảnh (Context) được cung cấp dưới đây. Nếu ngữ cảnh không chứa thông tin để trả lời, hãy lịch sự từ chối trả lời và nói rằng câu hỏi nằm ngoài phạm vi tài liệu.";
            
            var contents = new List<object>();
            
            // Add conversation history
            foreach (var msg in history.TakeLast(6))
            {
                contents.Add(new
                {
                    role = msg.Sender.ToLower() == "bot" ? "model" : "user",
                    parts = new[] { new { text = msg.Content } }
                });
            }

            // Add current message with context
            string promptText = $"Context:\n{context}\n\nQuestion: {query}";
            contents.Add(new
            {
                role = "user",
                parts = new[] { new { text = promptText } }
            });

            var requestBody = new
            {
                systemInstruction = new
                {
                    parts = new[] { new { text = systemInstruction } }
                },
                contents = contents,
                generationConfig = new
                {
                    temperature = 0.2
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
            };

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseString);
            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return text ?? "Lỗi phản hồi từ AI.";
        }

        private async Task<string> CallOpenAiApi(string query, string context, List<ChatMessage> history, string apiKey)
        {
            string url = "https://api.openai.com/v1/chat/completions";
            
            var messages = new List<object>
            {
                new { role = "system", content = "Bạn là trợ lý học tập thông minh. Hãy trả lời câu hỏi dựa TRÊN VÀ CHỈ DỰA TRÊN ngữ cảnh được cung cấp. Nếu ngữ cảnh không chứa thông tin, hãy nói câu hỏi nằm ngoài phạm vi tài liệu." }
            };

            foreach (var msg in history.TakeLast(6))
            {
                messages.Add(new { role = msg.Sender.ToLower() == "bot" ? "assistant" : "user", content = msg.Content });
            }

            messages.Add(new { role = "user", content = $"Context:\n{context}\n\nQuestion: {query}" });

            var requestBody = new
            {
                model = "gpt-4o-mini",
                messages = messages,
                temperature = 0.2
            };

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Authorization", $"Bearer {apiKey}");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseString);
            var text = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return text ?? "Lỗi phản hồi từ OpenAI.";
        }

        private string CompileOfflineResponse(string query, List<DocumentChunk> chunks)
        {
            // Parse query words
            string[] queryTerms = query.ToLower()
                .Split(new[] { ' ', '?', '.', ',', '!', '-' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(t => t.Length > 2)
                .ToArray();

            // Find the most relevant sentence inside the chunks containing query keywords
            var sentenceMatches = new List<(string Sentence, string DocName, int MatchCount)>();

            foreach (var chunk in chunks)
            {
                string docName = chunk.Document != null ? chunk.Document.FileName : "Tài liệu";
                // Split chunk content into sentences
                string[] sentences = Regex.Split(chunk.Content, @"(?<=[.!?])\s+");

                foreach (var sentence in sentences)
                {
                    if (string.IsNullOrWhiteSpace(sentence) || sentence.Length < 10) continue;
                    
                    int matches = queryTerms.Count(term => sentence.ToLower().Contains(term));
                    if (matches > 0)
                    {
                        sentenceMatches.Add((sentence.Trim(), docName, matches));
                    }
                }
            }

            if (!sentenceMatches.Any())
            {
                // Strict RAG boundary: if no sentence matching query words is found, reject
                return "Dựa trên tài liệu môn học hiện tại, tôi không tìm thấy thông tin cụ thể để trả lời câu hỏi này. Bạn vui lòng điều chỉnh câu hỏi tập trung vào nội dung tài liệu hoặc tải lên tài liệu học tập mới có liên quan.";
            }

            // Build response using the highest matching sentences
            var topMatches = sentenceMatches
                .OrderByDescending(s => s.MatchCount)
                .ThenByDescending(s => s.Sentence.Length)
                .Take(3)
                .ToList();

            var responseBuilder = new StringBuilder();
            responseBuilder.AppendLine("Dựa theo các tài liệu đã được tải lên của môn học, tôi xin thông tin tới bạn:");
            responseBuilder.AppendLine();

            foreach (var match in topMatches)
            {
                responseBuilder.AppendLine($"- {match.Sentence} *(Nguồn: **{match.DocName}**)*");
                responseBuilder.AppendLine();
            }

            responseBuilder.AppendLine("Hy vọng thông tin này giúp ích cho việc học của bạn! Nếu bạn cần tìm hiểu sâu hơn, hãy đặt câu hỏi chi tiết hơn dựa trên nguồn này.");

            return responseBuilder.ToString();
        }
    }
}

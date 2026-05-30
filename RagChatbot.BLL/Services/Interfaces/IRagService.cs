using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RagChatbot.DAL.Entities;

namespace RagChatbot.BLL.Services.Interfaces
{
    public interface IRagService
    {
        Task<bool> ProcessDocumentAsync(Guid documentId, string physicalPath, string embeddingModel = "multilingual-e5-base", string chunkingStrategy = "Recursive Character", int chunkSize = 500);
        IEnumerable<DocumentChunk> SearchRelevantChunks(Guid subjectId, string query, string embeddingModel = "multilingual-e5-base", int topK = 3);
        double CalculateCosineSimilarity(float[] vectorA, float[] vectorB);
        float[] GenerateEmbedding(string text, string modelName);
        List<string> ChunkText(string text, int chunkSize, string strategy);
    }
}

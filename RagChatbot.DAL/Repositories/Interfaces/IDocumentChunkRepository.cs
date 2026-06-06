using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RagChatbot.DAL.Entities;

namespace RagChatbot.DAL.Repositories.Interfaces
{
    public interface IDocumentChunkRepository
    {
        IEnumerable<DocumentChunk> GetByDocumentId(Guid documentId);
        int CountByDocumentId(Guid documentId);

        // Tìm các chunk gần nhất với vector câu hỏi trong phạm vi 1 môn học
        Task<IEnumerable<string>> SearchSimilarChunksAsync(Guid subjectId, float[] queryVector, int topK = 3);
    }
}

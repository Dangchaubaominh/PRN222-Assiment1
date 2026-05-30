using System;
using System.Collections.Generic;
using RagChatbot.DAL.Entities;

namespace RagChatbot.DAL.Repositories.Interfaces
{
    public interface IDocumentChunkRepository
    {
        IEnumerable<DocumentChunk> GetChunksByDocumentId(Guid documentId);
        IEnumerable<DocumentChunk> GetChunksBySubjectId(Guid subjectId);
        void AddChunks(IEnumerable<DocumentChunk> chunks);
        void DeleteChunksByDocumentId(Guid documentId);
    }
}

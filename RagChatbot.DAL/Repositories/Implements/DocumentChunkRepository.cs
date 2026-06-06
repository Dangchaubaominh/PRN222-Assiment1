using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using RagChatbot.DAL.Data;
using RagChatbot.DAL.Entities;
using RagChatbot.DAL.Repositories.Interfaces;

namespace RagChatbot.DAL.Repositories.Implements
{
    public class DocumentChunkRepository : IDocumentChunkRepository
    {
        private readonly ApplicationDbContext _context;

        public DocumentChunkRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<DocumentChunk> GetByDocumentId(Guid documentId)
        {
            return _context.DocumentChunks
                           .Where(c => c.DocumentId == documentId)
                           .ToList();
        }

        public int CountByDocumentId(Guid documentId)
        {
            return _context.DocumentChunks.Count(c => c.DocumentId == documentId);
        }

        public async Task<IEnumerable<string>> SearchSimilarChunksAsync(Guid subjectId, float[] queryVector, int topK = 3)
        {
            var vector = new Vector(queryVector);

            return await _context.DocumentChunks
                .Include(c => c.Document)
                .Where(c => c.Document.SubjectId == subjectId)
                .OrderBy(c => c.Embedding.CosineDistance(vector))
                .Take(topK)
                .Select(c => c.TextContent)
                .ToListAsync();
        }
    }
}

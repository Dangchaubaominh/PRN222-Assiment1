using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
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

        public IEnumerable<DocumentChunk> GetChunksByDocumentId(Guid documentId)
        {
            return _context.DocumentChunks
                           .Where(c => c.DocumentId == documentId)
                           .OrderBy(c => c.ChunkIndex)
                           .ToList();
        }

        public IEnumerable<DocumentChunk> GetChunksBySubjectId(Guid subjectId)
        {
            // Join Documents and DocumentChunks tables to get all chunks under a subject
            return _context.DocumentChunks
                           .Include(c => c.Document)
                           .Where(c => c.Document != null && c.Document.SubjectId == subjectId)
                           .ToList();
        }

        public void AddChunks(IEnumerable<DocumentChunk> chunks)
        {
            _context.DocumentChunks.AddRange(chunks);
            _context.SaveChanges();
        }

        public void DeleteChunksByDocumentId(Guid documentId)
        {
            var chunks = _context.DocumentChunks.Where(c => c.DocumentId == documentId);
            _context.DocumentChunks.RemoveRange(chunks);
            _context.SaveChanges();
        }
    }
}

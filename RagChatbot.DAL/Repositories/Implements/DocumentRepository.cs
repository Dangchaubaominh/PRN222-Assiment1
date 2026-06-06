using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RagChatbot.DAL.Data;
using RagChatbot.DAL.Entities;
using RagChatbot.DAL.Repositories.Interfaces;

namespace RagChatbot.DAL.Repositories.Implements
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly ApplicationDbContext _context;

        public DocumentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Document> GetDocumentsBySubjectId(Guid subjectId)
        {
            // Tìm tất cả tài liệu thuộc về 1 môn học cụ thể, mới nhất lên đầu
            return _context.Documents
                           .Where(d => d.SubjectId == subjectId)
                           .OrderByDescending(d => d.UploadedAt)
                           .ToList();
        }

        public Document GetById(Guid id)
        {
            return _context.Documents.Find(id);
        }

        public void Add(Document document)
        {
            _context.Documents.Add(document);
            _context.SaveChanges();
        }

        public void Delete(Guid id)
        {
            var document = _context.Documents.Find(id);
            if (document != null)
            {
                _context.Documents.Remove(document);
                _context.SaveChanges();
            }
        }

        public void UpdateStatus(Guid id, DocumentStatus newStatus)
        {
            var document = _context.Documents.Find(id);
            if (document != null)
            {
                document.Status = newStatus;
                _context.SaveChanges();
            }
        }

        public bool ExistsByFileName(Guid subjectId, string fileName)
        {
            return _context.Documents.Any(d =>
                d.SubjectId == subjectId &&
                d.FileName == fileName);
        }
    }
}
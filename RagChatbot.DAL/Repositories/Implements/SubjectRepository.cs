using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RagChatbot.DAL.Data;
using RagChatbot.DAL.Entities;
using RagChatbot.DAL.Repositories.Interfaces;

namespace RagChatbot.DAL.Repositories.Implements
{
    public class SubjectRepository : ISubjectRepository
    {
        private readonly ApplicationDbContext _context;

        // Dependency Injection: Nhận DbContext từ hệ thống
        public SubjectRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Subject> GetAll()
        {
            // Dùng LINQ sắp xếp môn học mới nhất lên đầu
            return _context.Subjects.OrderByDescending(s => s.CreatedAt).ToList();
        }

        public IEnumerable<Subject> SearchByName(string keyword)
        {
            // Dùng LINQ tìm kiếm theo tên
            if (string.IsNullOrEmpty(keyword)) return GetAll();
            return _context.Subjects
                           .Where(s => s.Name.ToLower().Contains(keyword.ToLower()))
                           .OrderBy(s => s.Name)
                           .ToList();
        }

        public Subject GetById(Guid id)
        {
            return _context.Subjects.Find(id);
        }

        public void Add(Subject subject)
        {
            _context.Subjects.Add(subject);
            _context.SaveChanges();
        }

        public void Update(Subject subject)
        {
            _context.Subjects.Update(subject);
            _context.SaveChanges();
        }

        public void Delete(Guid id)
        {
            var subject = _context.Subjects.Find(id);
            if (subject != null)
            {
                _context.Subjects.Remove(subject);
                _context.SaveChanges();
            }
        }
    }
}
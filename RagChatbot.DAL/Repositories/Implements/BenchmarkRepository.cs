using System;
using System.Collections.Generic;
using System.Linq;
using RagChatbot.DAL.Data;
using RagChatbot.DAL.Entities;
using RagChatbot.DAL.Repositories.Interfaces;

namespace RagChatbot.DAL.Repositories.Implements
{
    public class BenchmarkRepository : IBenchmarkRepository
    {
        private readonly ApplicationDbContext _context;

        public BenchmarkRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<BenchmarkResult> GetAll()
        {
            return _context.BenchmarkResults.OrderBy(r => r.RunAt).ToList();
        }

        public void Add(BenchmarkResult result)
        {
            _context.BenchmarkResults.Add(result);
            _context.SaveChanges();
        }

        public void ClearAll()
        {
            var results = _context.BenchmarkResults.ToList();
            _context.BenchmarkResults.RemoveRange(results);
            _context.SaveChanges();
        }
    }
}

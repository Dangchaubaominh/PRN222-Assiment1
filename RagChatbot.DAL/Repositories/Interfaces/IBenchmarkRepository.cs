using System;
using System.Collections.Generic;
using RagChatbot.DAL.Entities;

namespace RagChatbot.DAL.Repositories.Interfaces
{
    public interface IBenchmarkRepository
    {
        IEnumerable<BenchmarkResult> GetAll();
        void Add(BenchmarkResult result);
        void ClearAll();
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RagChatbot.DAL.Entities;

namespace RagChatbot.BLL.Services.Interfaces
{
    public interface IBenchmarkService
    {
        Task<bool> RunFullSuiteAsync(Guid subjectId);
        IEnumerable<BenchmarkResult> GetHistoricalResults();
        void ClearResults();
        List<QuestionGroundTruth> GetTestSet();
    }

    public class QuestionGroundTruth
    {
        public string Question { get; set; } = string.Empty;
        public string GroundTruthAnswer { get; set; } = string.Empty;
        public string GroundTruthContext { get; set; } = string.Empty;
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RagChatbot.BLL.Services.Interfaces;

namespace RagChatbot.MVC.Controllers
{
    [Authorize]
    public class BenchmarkController : Controller
    {
        private readonly IBenchmarkService _benchmarkService;
        private readonly ISubjectService _subjectService;

        public BenchmarkController(IBenchmarkService benchmarkService, ISubjectService subjectService)
        {
            _benchmarkService = benchmarkService;
            _subjectService = subjectService;
        }

        // GET: Hiển thị trang Dashboard thực nghiệm RBL
        [HttpGet]
        public IActionResult Index()
        {
            var subjects = _subjectService.GetAllSubjects().ToList();
            ViewBag.Subjects = subjects;

            var results = _benchmarkService.GetHistoricalResults().ToList();
            return View(results);
        }

        // POST: Kích hoạt chạy đánh giá thử nghiệm
        [HttpPost]
        public async Task<IActionResult> Run(Guid subjectId)
        {
            var success = await _benchmarkService.RunFullSuiteAsync(subjectId);
            if (success)
            {
                TempData["SuccessMessage"] = "Đã chạy thành công 50 câu hỏi thử nghiệm trên các cấu hình!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể chạy thử nghiệm do thiếu tài liệu được index cho môn học này.";
            }
            return RedirectToAction("Index");
        }

        // GET: Xem tập dữ liệu test 50 câu hỏi và ground truth
        [HttpGet]
        public IActionResult TestSet()
        {
            var testSet = _benchmarkService.GetTestSet();
            return View(testSet);
        }

        // POST: Xóa lịch sử kết quả thực nghiệm
        [HttpPost]
        public IActionResult Clear()
        {
            _benchmarkService.ClearResults();
            TempData["SuccessMessage"] = "Đã xóa toàn bộ số liệu thực nghiệm cũ.";
            return RedirectToAction("Index");
        }
    }
}

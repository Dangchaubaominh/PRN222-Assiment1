using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RagChatbot.BLL.Services.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RagChatbot.MVC.Controllers
{
    [Authorize]
    public class DocumentController : Controller
    {
        private readonly IDocumentService _documentService;
        private readonly ISubjectService _subjectService;
        private readonly IWebHostEnvironment _env;
        private readonly IDocumentProcessingService _docProcessingService;

        public DocumentController(
            IDocumentService documentService,
            ISubjectService subjectService,
            IWebHostEnvironment env,
            IDocumentProcessingService docProcessingService)
        {
            _documentService = documentService;
            _subjectService = subjectService;
            _env = env;
            _docProcessingService = docProcessingService;
        }

        public IActionResult Index(Guid subjectId)
        {
            var subject = _subjectService.GetSubjectById(subjectId);
            if (subject == null) return NotFound();

            ViewBag.Subject = subject;
            var documents = _documentService.GetDocumentsBySubject(subjectId);
            return View(documents);
        }

        [Authorize(Roles = "Admin, Lecturer")]
        [HttpGet]
        public IActionResult Create(Guid subjectId)
        {
            ViewBag.SubjectId = subjectId;
            return View();
        }

        [Authorize(Roles = "Admin, Lecturer")]
        [HttpPost]
        public async Task<IActionResult> Create(Guid subjectId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Vui lòng chọn một file hợp lệ để tải lên.");
                ViewBag.SubjectId = subjectId;
                return View();
            }

            string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");

            using (var stream = file.OpenReadStream())
            {
                var result = await _documentService.UploadDocumentAsync(subjectId, file.FileName, stream, uploadsFolder);

                switch (result)
                {
                    case DocumentUploadResult.Duplicate:
                        ModelState.AddModelError("", $"Tài liệu \"{file.FileName}\" đã tồn tại trong môn học này. Vui lòng đổi tên file hoặc xóa tài liệu cũ trước khi upload lại.");
                        ViewBag.SubjectId = subjectId;
                        return View();

                    case DocumentUploadResult.Error:
                        ModelState.AddModelError("", "Có lỗi xảy ra khi lưu file. Vui lòng thử lại.");
                        ViewBag.SubjectId = subjectId;
                        return View();

                    case DocumentUploadResult.Success:
                        var uploadedDoc = _documentService.GetDocumentsBySubject(subjectId)
                                                          .FirstOrDefault(d => d.FileName == file.FileName);
                        if (uploadedDoc != null)
                        {
                            bool isProcessed = await _docProcessingService.ProcessDocumentAsync(uploadedDoc.Id, _env.WebRootPath);
                            if (isProcessed)
                                TempData["SuccessMessage"] = "Upload và nạp tri thức cho AI thành công!";
                            else
                                TempData["WarningMessage"] = "Upload thành công nhưng AI không thể đọc nội dung file này.";
                        }
                        return RedirectToAction("Index", new { subjectId });
                }
            }

            return RedirectToAction("Index", new { subjectId });
        }

        [Authorize(Roles = "Admin, Lecturer")]
        [HttpPost]
        public IActionResult Delete(Guid id, Guid subjectId)
        {
            var doc = _documentService.GetDocumentById(id);
            string fileName = doc?.FileName ?? "Tài liệu";
            _documentService.DeleteDocument(id, _env.WebRootPath);
            TempData["SuccessMessage"] = $"Đã xóa tài liệu \"{fileName}\" thành công.";
            return RedirectToAction("Index", new { subjectId });
        }

        [HttpGet]
        public IActionResult Download(Guid id)
        {
            var document = _documentService.GetDocumentById(id);
            if (document == null) return NotFound("Không tìm thấy thông tin tài liệu.");

            string physicalPath = Path.Combine(_env.WebRootPath, document.FilePath.TrimStart('/'));
            if (!System.IO.File.Exists(physicalPath))
                return NotFound("File gốc không còn tồn tại trên hệ thống.");

            return PhysicalFile(physicalPath, "application/octet-stream", document.FileName);
        }

        [HttpGet]
        public IActionResult ViewDoc(Guid id)
        {
            var document = _documentService.GetDocumentById(id);
            if (document == null) return NotFound("Tài liệu không tồn tại.");

            var fileExtension = Path.GetExtension(document.FileName).ToLower();
            if (fileExtension == ".txt")
            {
                string physicalPath = Path.Combine(_env.WebRootPath, document.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(physicalPath))
                    ViewBag.FileContent = System.IO.File.ReadAllText(physicalPath);
            }

            // Load các chunk AI đã học từ tài liệu này
            var chunks = _documentService.GetChunksByDocumentId(id).ToList();
            ViewBag.Chunks = chunks;
            ViewBag.TotalWords = chunks.Sum(c => c.WordCount);

            return View(document);
        }
    }
}

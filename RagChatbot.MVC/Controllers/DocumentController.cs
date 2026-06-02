using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using RagChatbot.BLL.Services.Interfaces;
using System;
using System.IO;
using System.Linq; // Thêm thư viện này để dùng LINQ tìm ID file
using System.Threading.Tasks;

namespace RagChatbot.MVC.Controllers
{
    public class DocumentController : Controller
    {
        private readonly IDocumentService _documentService;
        private readonly ISubjectService _subjectService;
        private readonly IWebHostEnvironment _env;

        // 1. Khai báo thêm Service xử lý AI
        private readonly IDocumentProcessingService _docProcessingService;

        public DocumentController(
            IDocumentService documentService,
            ISubjectService subjectService,
            IWebHostEnvironment env,
            IDocumentProcessingService docProcessingService) // 2. Tiêm vào Constructor
        {
            _documentService = documentService;
            _subjectService = subjectService;
            _env = env;
            _docProcessingService = docProcessingService; // 3. Gán giá trị
        }

        // Hiện danh sách tài liệu của một Môn học cụ thể
        public IActionResult Index(Guid subjectId)
        {
            var subject = _subjectService.GetSubjectById(subjectId);
            if (subject == null) return NotFound();

            ViewBag.Subject = subject; // Truyền thông tin môn học sang View
            var documents = _documentService.GetDocumentsBySubject(subjectId);
            return View(documents);
        }

        // Mở form Upload
        [HttpGet]
        public IActionResult Create(Guid subjectId)
        {
            ViewBag.SubjectId = subjectId;
            return View();
        }

        // Nhận file từ giao diện gửi lên
        [HttpPost]
        public async Task<IActionResult> Create(Guid subjectId, IFormFile file)
        {
            // Kiểm tra xem người dùng có chọn file chưa
            if (file != null && file.Length > 0)
            {
                // Đường dẫn lưu file: wwwroot/uploads
                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");

                using (var stream = file.OpenReadStream())
                {
                    // Hàm cũ của bạn: Lưu file vật lý và lưu DB
                    var isSuccess = await _documentService.UploadDocumentAsync(subjectId, file.FileName, stream, uploadsFolder);

                    if (isSuccess)
                    {
                        // =========================================================
                        // BẮT ĐẦU KÍCH HOẠT AI (RAG PIPELINE)
                        // =========================================================

                        // Mẹo: Vì hàm Upload của bạn trả về true/false, ta tìm lại file vừa lưu để lấy ID
                        var uploadedDoc = _documentService.GetDocumentsBySubject(subjectId)
                                                          .FirstOrDefault(d => d.FileName == file.FileName);

                        if (uploadedDoc != null)
                        {
                            // Gọi BLL mở file ra, băm nhỏ và gửi lên OpenAI lấy Vector
                            bool isProcessed = await _docProcessingService.ProcessDocumentAsync(uploadedDoc.Id, _env.WebRootPath);

                            if (isProcessed)
                                TempData["SuccessMessage"] = "Upload và nạp tri thức cho AI thành công!";
                            else
                                TempData["WarningMessage"] = "Upload thành công nhưng AI không thể đọc nội dung file này.";
                        }
                        // =========================================================

                        return RedirectToAction("Index", new { subjectId = subjectId });
                    }
                }
            }
            ModelState.AddModelError("", "Vui lòng chọn một file hợp lệ để tải lên.");
            ViewBag.SubjectId = subjectId;
            return View();
        }

        // Xóa tài liệu
        [HttpPost]
        public IActionResult Delete(Guid id, Guid subjectId)
        {
            _documentService.DeleteDocument(id, _env.WebRootPath);
            return RedirectToAction("Index", new { subjectId = subjectId });
        }

        // Cấp lệnh Tải file về máy (Ép buộc tải xuống thay vì mở trên trình duyệt)
        [HttpGet]
        public IActionResult Download(Guid id)
        {
            // 1. Tìm thông tin tài liệu trong Database
            var document = _documentService.GetDocumentById(id);
            if (document == null) return NotFound("Không tìm thấy thông tin tài liệu.");

            // 2. Định vị file thật nằm ở đâu trên ổ cứng máy chủ
            string physicalPath = Path.Combine(_env.WebRootPath, document.FilePath.TrimStart('/'));

            // 3. Kiểm tra xem file có bị ai đó lỡ tay xóa trong ổ cứng chưa
            if (!System.IO.File.Exists(physicalPath))
            {
                return NotFound("File gốc không còn tồn tại trên hệ thống.");
            }

            // 4. Trả file về cho trình duyệt kèm theo tên gốc (FileName) để người dùng lưu lại
            return PhysicalFile(physicalPath, "application/octet-stream", document.FileName);
        }

        // GET: Mở trang xem tài liệu (Viewer)
        [HttpGet]
        public IActionResult ViewDoc(Guid id)
        {
            var document = _documentService.GetDocumentById(id);
            if (document == null) return NotFound("Tài liệu không tồn tại.");

            // Xử lý riêng cho file TXT: Đọc nội dung chữ bên trong file
            var fileExtension = Path.GetExtension(document.FileName).ToLower();
            if (fileExtension == ".txt")
            {
                string physicalPath = Path.Combine(_env.WebRootPath, document.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(physicalPath))
                {
                    // Đọc toàn bộ chữ và gửi sang View
                    ViewBag.FileContent = System.IO.File.ReadAllText(physicalPath);
                }
            }

            return View(document);
        }
    }
}
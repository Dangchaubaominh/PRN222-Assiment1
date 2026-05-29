using Microsoft.AspNetCore.Mvc;
using RagChatbot.BLL.Services.Interfaces;
using RagChatbot.DAL.Entities;

namespace RagChatbot.MVC.Controllers
{
    public class SubjectController : Controller
    {
        private readonly ISubjectService _subjectService;

        public SubjectController(ISubjectService subjectService)
        {
            _subjectService = subjectService;
        }

        // Hiện danh sách
        public IActionResult Index()
        {
            var subjects = _subjectService.GetAllSubjects();
            return View(subjects);
        }

        // Mở Form thêm mới
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // GET: Mở form Sửa (Lấy thông tin cũ điền vào form)
        [HttpGet]
        public IActionResult Edit(Guid id)
        {
            var subject = _subjectService.GetSubjectById(id);
            if (subject == null)
            {
                return NotFound();
            }
            return View(subject);
        }

        // Nhận dữ liệu từ Form gửi lên
        [HttpPost]
        public IActionResult Create(Subject subject)
        {
            ModelState.Remove("Documents");
            if (ModelState.IsValid)
            {
                var isSuccess = _subjectService.CreateSubject(subject);
                if (isSuccess)
                {
                    // Thêm thành công thì quay về trang danh sách
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError("", "Tạo môn học thất bại do lỗi logic.");
            }
            return View(subject);
        }
        // POST: Xử lý chức năng Xóa
        [HttpPost]
        public IActionResult Delete(Guid id)
        {
            _subjectService.DeleteSubject(id);
            return RedirectToAction("Index");
        }
    }
}
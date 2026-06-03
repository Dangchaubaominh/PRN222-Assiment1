using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RagChatbot.BLL.Services.Interfaces;
using System;
using System.Security.Claims;

namespace RagChatbot.MVC.Controllers
{
    [Authorize(Roles = "Admin, Lecturer")]
    public class MemberController : Controller
    {
        private readonly IUserSubjectService _userSubjectService;
        private readonly ISubjectService _subjectService;

        public MemberController(IUserSubjectService userSubjectService, ISubjectService subjectService)
        {
            _userSubjectService = userSubjectService;
            _subjectService = subjectService;
        }

        // Danh sách thành viên của môn học
        public IActionResult Index(Guid subjectId)
        {
            var subject = _subjectService.GetSubjectById(subjectId);
            if (subject == null) return NotFound();

            // Giảng viên chỉ quản lý môn học mình được gán
            if (User.IsInRole("Lecturer"))
            {
                int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var assigned = _userSubjectService.GetAssignedSubjects(userId);
                bool isMine = false;
                foreach (var s in assigned)
                    if (s.Id == subjectId) { isMine = true; break; }

                if (!isMine) return Forbid();
            }

            ViewBag.Subject = subject;
            var members = _userSubjectService.GetAssignedUsers(subjectId);
            return View(members);
        }

        // Form thêm thành viên
        [HttpGet]
        public IActionResult Add(Guid subjectId)
        {
            var subject = _subjectService.GetSubjectById(subjectId);
            if (subject == null) return NotFound();

            string requesterRole = User.IsInRole("Admin") ? "Admin" : "Lecturer";
            var addableUsers = _userSubjectService.GetAddableUsers(subjectId, requesterRole);

            ViewBag.Subject = subject;
            return View(addableUsers);
        }

        // Thực hiện gán thành viên
        [HttpPost]
        public IActionResult Add(Guid subjectId, int userId)
        {
            // Lecturer chỉ được thêm Student
            if (User.IsInRole("Lecturer"))
            {
                var addable = _userSubjectService.GetAddableUsers(subjectId, "Lecturer");
                bool allowed = false;
                foreach (var u in addable)
                    if (u.Id == userId) { allowed = true; break; }

                if (!allowed) return Forbid();
            }

            _userSubjectService.Assign(userId, subjectId);
            return RedirectToAction("Index", new { subjectId });
        }

        // Xóa thành viên khỏi môn học
        [HttpPost]
        public IActionResult Remove(Guid subjectId, int userId)
        {
            // Lecturer chỉ được xóa Student
            if (User.IsInRole("Lecturer"))
            {
                var members = _userSubjectService.GetAssignedUsers(subjectId);
                foreach (var m in members)
                {
                    if (m.Id == userId && m.Role != "Student")
                        return Forbid();
                }
            }

            _userSubjectService.Remove(userId, subjectId);
            return RedirectToAction("Index", new { subjectId });
        }
    }
}

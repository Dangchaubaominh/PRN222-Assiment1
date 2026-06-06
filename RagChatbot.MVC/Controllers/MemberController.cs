using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RagChatbot.BLL.Services.Implements;
using RagChatbot.BLL.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
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
            ViewBag.TeacherCount = _userSubjectService.CountTeachersInSubject(subjectId);
            ViewBag.TeacherLimit = UserSubjectService.MaxTeachersPerSubject;
            return View(addableUsers);
        }

        // Thêm nhiều thành viên cùng lúc
        [HttpPost]
        public IActionResult Add(Guid subjectId, List<int> selectedUserIds)
        {
            if (selectedUserIds == null || selectedUserIds.Count == 0)
                return RedirectToAction("Add", new { subjectId });

            if (User.IsInRole("Lecturer"))
            {
                var allowedIds = _userSubjectService.GetAddableUsers(subjectId, "Lecturer")
                                                    .Select(u => u.Id).ToHashSet();
                if (!selectedUserIds.All(id => allowedIds.Contains(id)))
                    return Forbid();
            }

            int added = 0, limitBlocked = 0;
            foreach (var userId in selectedUserIds)
            {
                var result = _userSubjectService.Assign(userId, subjectId);
                if (result == AssignResult.Success) added++;
                else if (result == AssignResult.TeacherLimitReached) limitBlocked++;
            }

            if (limitBlocked > 0 && added == 0)
                TempData["ErrorMessage"] = $"Không thể thêm: môn học đã đạt tối đa {UserSubjectService.MaxTeachersPerSubject} giảng viên.";
            else if (limitBlocked > 0)
                TempData["WarningMessage"] = $"Đã thêm {added} thành viên. {limitBlocked} giảng viên bị từ chối vì môn học đã đạt giới hạn {UserSubjectService.MaxTeachersPerSubject} giảng viên.";
            else
                TempData["SuccessMessage"] = $"Đã thêm {added} thành viên vào môn học.";

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
            TempData["SuccessMessage"] = "Đã xóa thành viên khỏi môn học.";
            return RedirectToAction("Index", new { subjectId });
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RagChatbot.BLL.Services.Interfaces;
using RagChatbot.BLL.DTOs;
using System;
using System.Security.Claims;

namespace RagChatbot.MVC.Controllers
{
    [Authorize]
    public class SubjectController : Controller
    {
        private readonly ISubjectService _subjectService;
        private readonly IUserSubjectService _userSubjectService;

        public SubjectController(ISubjectService subjectService, IUserSubjectService userSubjectService)
        {
            _subjectService = subjectService;
            _userSubjectService = userSubjectService;
        }

        public IActionResult Index()
        {
            if (User.IsInRole("Admin"))
                return View(_subjectService.GetAllSubjects());

            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            return View(_userSubjectService.GetAssignedSubjects(userId));
        }

        [Authorize(Roles = "Admin, Lecturer")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Admin, Lecturer")]
        [HttpPost]
        public IActionResult Create(SubjectDto subjectDto)
        {
            if (ModelState.IsValid)
            {
                _subjectService.CreateSubject(subjectDto);
                return RedirectToAction("Index");
            }
            return View(subjectDto);
        }

        [Authorize(Roles = "Admin, Lecturer")]
        [HttpGet]
        public IActionResult Edit(Guid id)
        {
            var subject = _subjectService.GetSubjectById(id);
            if (subject == null) return NotFound();
            return View(subject);
        }

        [Authorize(Roles = "Admin, Lecturer")]
        [HttpPost]
        public IActionResult Edit(SubjectDto subjectDto)
        {
            if (ModelState.IsValid)
            {
                _subjectService.UpdateSubject(subjectDto);
                return RedirectToAction("Index");
            }
            return View(subjectDto);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Delete(Guid id)
        {
            _subjectService.DeleteSubject(id);
            return RedirectToAction("Index");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RagChatbot.BLL.Services.Interfaces;
using RagChatbot.DAL.Entities;
using RagChatbot.DAL.Repositories.Interfaces;

namespace RagChatbot.MVC.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly IChatRepository _chatRepository;
        private readonly ISubjectService _subjectService;
        private readonly IAiService _aiService;

        public ChatController(IChatRepository chatRepository, ISubjectService subjectService, IAiService aiService)
        {
            _chatRepository = chatRepository;
            _subjectService = subjectService;
            _aiService = aiService;
        }

        // GET: Hiển thị giao diện Chat chính
        [HttpGet]
        public IActionResult Index(Guid? subjectId, Guid? sessionId)
        {
            var subjects = _subjectService.GetAllSubjects().ToList();
            ViewBag.Subjects = subjects;

            // Lấy ID người dùng hiện tại
            var accountIdClaim = User.FindFirst("AccountId");
            if (accountIdClaim == null) return RedirectToAction("Login", "Account");
            var accountId = Guid.Parse(accountIdClaim.Value);

            Guid activeSubjectId = Guid.Empty;
            if (subjectId.HasValue)
            {
                activeSubjectId = subjectId.Value;
            }
            else if (subjects.Any())
            {
                activeSubjectId = subjects.First().Id;
            }

            ViewBag.ActiveSubjectId = activeSubjectId;

            // Lấy danh sách các session chat cũ của người dùng này trong môn học
            var sessions = _chatRepository.GetSessionsByAccountAndSubject(accountId, activeSubjectId).ToList();
            ViewBag.Sessions = sessions;

            ChatSession? activeSession = null;
            if (sessionId.HasValue)
            {
                activeSession = _chatRepository.GetSessionById(sessionId.Value);
            }
            else if (sessions.Any())
            {
                activeSession = _chatRepository.GetSessionById(sessions.First().Id);
            }

            return View(activeSession);
        }

        // POST: Tạo phiên chat mới
        [HttpPost]
        public IActionResult CreateSession(Guid subjectId, string title)
        {
            var accountIdClaim = User.FindFirst("AccountId");
            if (accountIdClaim == null) return Challenge();
            var accountId = Guid.Parse(accountIdClaim.Value);

            string sessionTitle = string.IsNullOrWhiteSpace(title) ? "Cuộc hội thoại mới" : title.Trim();

            var newSession = new ChatSession
            {
                Id = Guid.NewGuid(),
                SubjectId = subjectId,
                AccountId = accountId,
                Title = sessionTitle,
                CreatedAt = DateTime.UtcNow
            };

            _chatRepository.AddSession(newSession);

            return RedirectToAction("Index", new { subjectId = subjectId, sessionId = newSession.Id });
        }

        // POST: Gửi tin nhắn và nhận phản hồi RAG
        [HttpPost]
        public async Task<IActionResult> SendMessage(Guid sessionId, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return BadRequest("Tin nhắn trống.");
            }

            var session = _chatRepository.GetSessionById(sessionId);
            if (session == null) return NotFound("Không tìm thấy phiên chat.");

            // 1. Lưu tin nhắn của Người dùng
            var userMsg = new ChatMessage
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                Sender = "User",
                Content = message,
                CreatedAt = DateTime.UtcNow
            };
            _chatRepository.AddMessage(userMsg);

            // Lấy lịch sử 6 tin nhắn gần nhất để tạo ngữ cảnh liên tiếp
            var history = _chatRepository.GetMessagesBySessionId(sessionId).ToList();

            // 2. Gọi AI để sinh câu trả lời (Có trích xuất RAG)
            var (answer, citations) = await _aiService.GenerateResponseAsync(
                session.SubjectId, 
                message, 
                history, 
                embeddingModel: "multilingual-e5-base"
            );

            // 3. Lưu tin nhắn của Bot kèm nguồn trích dẫn
            var botMsg = new ChatMessage
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                Sender = "Bot",
                Content = answer,
                CitationsJson = JsonSerializer.Serialize(citations),
                CreatedAt = DateTime.UtcNow
            };
            _chatRepository.AddMessage(botMsg);

            return Json(new
            {
                success = true,
                userMessage = new { content = userMsg.Content, time = userMsg.CreatedAt.ToLocalTime().ToString("HH:mm") },
                botMessage = new { content = botMsg.Content, citations = citations, time = botMsg.CreatedAt.ToLocalTime().ToString("HH:mm") }
            });
        }

        // POST: Xóa phiên chat
        [HttpPost]
        public IActionResult DeleteSession(Guid sessionId, Guid subjectId)
        {
            _chatRepository.DeleteSession(sessionId);
            return RedirectToAction("Index", new { subjectId = subjectId });
        }
    }
}

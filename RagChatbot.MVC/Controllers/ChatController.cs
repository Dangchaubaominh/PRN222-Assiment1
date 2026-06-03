using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RagChatbot.BLL.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace RagChatbot.MVC.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly IChatbotService _chatbotService;

        // Chỉ tiêm duy nhất ChatbotService
        public ChatController(IChatbotService chatbotService)
        {
            _chatbotService = chatbotService;
        }

        [HttpGet]
        public IActionResult Index(Guid subjectId)
        {
            ViewBag.SubjectId = subjectId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromForm] Guid subjectId, [FromForm] string userMessage)
            {
            // Ủy thác toàn bộ việc tính toán và lấy câu trả lời cho BLL
            string aiAnswer = await _chatbotService.GetAnswerAsync(subjectId, userMessage);

            return Json(new { reply = aiAnswer });
        }
    }
}
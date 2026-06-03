using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RagChatbot.BLL.DTOs;
using RagChatbot.BLL.Services.Interfaces;

namespace RagChatbot.MVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        public IActionResult Index()
        {
            var users = _userService.GetAllUsers();
            return View(users);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(UserManageDto dto)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrWhiteSpace(dto.Password))
                {
                    ModelState.AddModelError("Password", "Mật khẩu không được để trống");
                    return View(dto);
                }
                _userService.CreateUser(dto);
                return RedirectToAction("Index");
            }
            return View(dto);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            string currentUsername = User.Identity.Name;
            bool success = _userService.DeleteUser(id, currentUsername);

            if (!success)
                TempData["ErrorMessage"] = "Không thể xóa tài khoản này (bạn không thể tự xóa chính mình).";

            return RedirectToAction("Index");
        }
    }
}

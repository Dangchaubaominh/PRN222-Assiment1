using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RagChatbot.BLL.DTOs;
using RagChatbot.BLL.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace RagChatbot.MVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly IUserService  _userService;
        private readonly IEmailService _emailService;

        public UserController(IUserService userService, IEmailService emailService)
        {
            _userService  = userService;
            _emailService = emailService;
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
        public async Task<IActionResult> Create(UserManageDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            if (string.IsNullOrWhiteSpace(dto.Password))
            {
                ModelState.AddModelError("Password", "Mật khẩu không được để trống");
                return View(dto);
            }

            // Lưu tài khoản vào DB
            _userService.CreateUser(dto);

            // Lấy thông tin admin đang đăng nhập để bind vào email
            string adminUsername = User.Identity!.Name!;
            var adminInfo = _userService.GetByUsername(adminUsername);
            string adminName  = adminInfo?.FullName ?? adminUsername;
            string adminEmail = adminInfo?.Email    ?? "";

            // Gửi email thông tin tài khoản (bắt buộc vì Email là required)
            string displayName = dto.FullName ?? dto.Username;
            bool sent = await _emailService.SendAccountCredentialsAsync(
                dto.Email, displayName, dto.Username, dto.Password,
                adminName, adminEmail);

            TempData["SuccessMessage"] = sent
                ? $"Đã tạo tài khoản và gửi thông tin đến {dto.Email} thành công."
                : $"Đã tạo tài khoản nhưng không thể gửi email đến {dto.Email}. Kiểm tra cấu hình SMTP.";

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var dto = _userService.GetEditById(id);
            if (dto == null) return NotFound();
            return View(dto);
        }

        [HttpPost]
        public IActionResult Edit(UserEditDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            bool success = _userService.UpdateUserInfo(dto);
            if (success)
                TempData["SuccessMessage"] = $"Đã cập nhật thông tin tài khoản \"{dto.Username}\" thành công.";
            else
                TempData["ErrorMessage"] = "Không tìm thấy tài khoản cần cập nhật.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            string currentUsername = User.Identity!.Name!;
            bool success = _userService.DeleteUser(id, currentUsername);

            if (!success)
                TempData["ErrorMessage"] = "Không thể xóa tài khoản này (bạn không thể tự xóa chính mình).";

            return RedirectToAction("Index");
        }
    }
}

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using RagChatbot.BLL.Services.Interfaces;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RagChatbot.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService  _userService;
        private readonly IEmailService _emailService;

        public AccountController(IUserService userService, IEmailService emailService)
        {
            _userService  = userService;
            _emailService = emailService;
        }

        // ── Login ──────────────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var userDto = _userService.Authenticate(username, password);
            if (userDto != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userDto.Id.ToString()),
                    new Claim(ClaimTypes.Name,      userDto.Username),
                    new Claim(ClaimTypes.GivenName, userDto.FullName ?? userDto.Username),
                    new Claim(ClaimTypes.Role,      userDto.Role)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                                             new ClaimsPrincipal(identity));
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Tài khoản hoặc mật khẩu không chính xác!";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public IActionResult AccessDenied() => View();

        // ── Forgot Password ────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            // Luôn hiển thị thông báo giống nhau (bảo mật — không tiết lộ email có tồn tại không)
            ViewBag.Sent = true;

            string token = _userService.GeneratePasswordResetToken(email);
            if (token != null)
            {
                // Tìm tên người dùng để cá nhân hoá email
                var user = _userService.GetAllUsers();
                string fullName = "";
                foreach (var u in user)
                    if (u.Email == email) { fullName = u.FullName ?? u.Username; break; }

                string resetLink = Url.Action("ResetPassword", "Account",
                                              new { token }, Request.Scheme);

                await _emailService.SendPasswordResetEmailAsync(email, fullName, resetLink);
            }

            return View();
        }

        // ── Reset Password ─────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (!_userService.IsValidResetToken(token))
            {
                ViewBag.Expired = true;
                return View();
            }

            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword(string token, string newPassword, string confirmPassword)
        {
            if (!_userService.IsValidResetToken(token))
            {
                ViewBag.Expired = true;
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.Token = token;
                ViewBag.Error = "Mật khẩu xác nhận không khớp.";
                return View();
            }

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 4)
            {
                ViewBag.Token = token;
                ViewBag.Error = "Mật khẩu phải có ít nhất 4 ký tự.";
                return View();
            }

            _userService.ResetPassword(token, newPassword);

            TempData["SuccessMessage"] = "Đặt lại mật khẩu thành công! Hãy đăng nhập bằng mật khẩu mới.";
            return RedirectToAction("Login");
        }
    }
}

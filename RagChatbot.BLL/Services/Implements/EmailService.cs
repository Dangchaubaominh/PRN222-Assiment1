using Microsoft.Extensions.Configuration;
using RagChatbot.BLL.Services.Interfaces;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace RagChatbot.BLL.Services.Implements
{
    public class EmailService : IEmailService
    {
        private readonly string _host;
        private readonly int    _port;
        private readonly string _username;
        private readonly string _password;
        private readonly string _fromName;

        public EmailService(IConfiguration configuration)
        {
            var smtp  = configuration.GetSection("Smtp");
            _host     = smtp["Host"]     ?? "smtp.gmail.com";
            _port     = int.TryParse(smtp["Port"], out var p) ? p : 587;
            _username = smtp["Username"] ?? "";
            _password = smtp["Password"] ?? "";
            _fromName = smtp["FromName"] ?? "EduChatbot";
        }

        public async Task<bool> SendAccountCredentialsAsync(string toEmail, string fullName,
                                                            string username, string password,
                                                            string adminName, string adminEmail)
        {
            try
            {
                using var client = new SmtpClient(_host, _port)
                {
                    EnableSsl        = true,
                    UseDefaultCredentials = false,
                    Credentials      = new NetworkCredential(_username, _password)
                };

                var mail = new MailMessage
                {
                    From       = new MailAddress(_username, $"{adminName} via {_fromName}"),
                    Subject    = "🎓 Thông tin tài khoản EduChatbot của bạn",
                    Body       = BuildHtmlBody(fullName, username, password, adminName, adminEmail),
                    IsBodyHtml = true
                };
                mail.To.Add(toEmail);

                // Nếu người nhận reply, email sẽ trả về Gmail của admin
                if (!string.IsNullOrWhiteSpace(adminEmail))
                    mail.ReplyToList.Add(new MailAddress(adminEmail, adminName));

                await client.SendMailAsync(mail);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string fullName, string resetLink)
        {
            try
            {
                using var client = new SmtpClient(_host, _port)
                {
                    EnableSsl             = true,
                    UseDefaultCredentials = false,
                    Credentials           = new NetworkCredential(_username, _password)
                };

                var mail = new MailMessage
                {
                    From       = new MailAddress(_username, _fromName),
                    Subject    = "🔑 Đặt lại mật khẩu EduChatbot",
                    Body       = BuildResetHtmlBody(fullName, resetLink),
                    IsBodyHtml = true
                };
                mail.To.Add(toEmail);

                await client.SendMailAsync(mail);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string BuildResetHtmlBody(string fullName, string resetLink) => $"""
            <!DOCTYPE html>
            <html lang="vi">
            <head><meta charset="UTF-8"><meta name="viewport" content="width=device-width,initial-scale=1"></head>
            <body style="margin:0;padding:0;background:#F1F5F9;font-family:'Segoe UI',Arial,sans-serif">
              <table width="100%" cellpadding="0" cellspacing="0" style="background:#F1F5F9;padding:32px 16px">
                <tr><td align="center">
                  <table width="560" cellpadding="0" cellspacing="0" style="background:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,.08)">

                    <!-- Header -->
                    <tr>
                      <td style="background:linear-gradient(135deg,#6366F1,#8B5CF6);padding:32px;text-align:center">
                        <div style="font-size:36px;margin-bottom:8px">🔑</div>
                        <h1 style="margin:0;color:#ffffff;font-size:24px;font-weight:800">Đặt lại mật khẩu</h1>
                        <p style="margin:6px 0 0;color:rgba(255,255,255,.8);font-size:13px">EduChatbot · AI-Powered Learning</p>
                      </td>
                    </tr>

                    <!-- Body -->
                    <tr>
                      <td style="padding:36px 40px">
                        <p style="margin:0 0 8px;font-size:15px;color:#374151">Xin chào <strong style="color:#111827">{fullName}</strong>,</p>
                        <p style="margin:0 0 28px;font-size:14px;color:#6B7280;line-height:1.6">
                          Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản EduChatbot của bạn.
                          Nhấn vào nút bên dưới để tạo mật khẩu mới:
                        </p>

                        <!-- CTA Button -->
                        <table width="100%" cellpadding="0" cellspacing="0" style="margin-bottom:28px">
                          <tr>
                            <td align="center">
                              <a href="{resetLink}"
                                 style="display:inline-block;background:linear-gradient(135deg,#6366F1,#8B5CF6);color:#ffffff;text-decoration:none;font-size:15px;font-weight:700;padding:14px 36px;border-radius:10px;letter-spacing:0.3px">
                                🔐 Đặt lại mật khẩu
                              </a>
                            </td>
                          </tr>
                        </table>

                        <!-- Expiry notice -->
                        <table width="100%" cellpadding="0" cellspacing="0"
                               style="background:#FFF7ED;border:1px solid #FED7AA;border-radius:10px;margin-bottom:28px">
                          <tr>
                            <td style="padding:14px 18px">
                              <p style="margin:0;font-size:13px;color:#92400E;line-height:1.6">
                                ⏰ <strong>Link có hiệu lực trong 30 phút.</strong>
                                Sau thời gian này, bạn cần yêu cầu lại từ trang đăng nhập.
                              </p>
                            </td>
                          </tr>
                        </table>

                        <!-- Fallback link -->
                        <p style="margin:0 0 8px;font-size:12px;color:#9CA3AF">Nếu nút không hoạt động, copy đường link sau vào trình duyệt:</p>
                        <p style="margin:0 0 24px;font-size:11px;color:#6366F1;word-break:break-all">{resetLink}</p>

                        <p style="margin:0;font-size:13px;color:#9CA3AF;line-height:1.7">
                          Nếu bạn không yêu cầu đặt lại mật khẩu, hãy bỏ qua email này.<br>
                          Tài khoản của bạn vẫn an toàn.
                        </p>
                      </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                      <td style="background:#F8FAFF;padding:20px 40px;text-align:center;border-top:1px solid #E5E7EB">
                        <p style="margin:0;font-size:11px;color:#9CA3AF">
                          EduChatbot · PRN222 Assignment · FPT University
                        </p>
                      </td>
                    </tr>

                  </table>
                </td></tr>
              </table>
            </body>
            </html>
            """;

        private static string BuildHtmlBody(string fullName, string username, string password,
                                             string adminName, string adminEmail)
        {
            return $"""
            <!DOCTYPE html>
            <html lang="vi">
            <head><meta charset="UTF-8"><meta name="viewport" content="width=device-width,initial-scale=1"></head>
            <body style="margin:0;padding:0;background:#F1F5F9;font-family:'Segoe UI',Arial,sans-serif">
              <table width="100%" cellpadding="0" cellspacing="0" style="background:#F1F5F9;padding:32px 16px">
                <tr><td align="center">
                  <table width="560" cellpadding="0" cellspacing="0" style="background:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,.08)">

                    <!-- Header -->
                    <tr>
                      <td style="background:linear-gradient(135deg,#6366F1,#8B5CF6);padding:32px;text-align:center">
                        <div style="font-size:36px;margin-bottom:8px">🎓</div>
                        <h1 style="margin:0;color:#ffffff;font-size:24px;font-weight:800;letter-spacing:-0.5px">EduChatbot</h1>
                        <p style="margin:6px 0 0;color:rgba(255,255,255,.8);font-size:13px">AI-Powered Learning Platform</p>
                      </td>
                    </tr>

                    <!-- Body -->
                    <tr>
                      <td style="padding:36px 40px">
                        <p style="margin:0 0 8px;font-size:15px;color:#374151">Xin chào <strong style="color:#111827">{fullName}</strong>,</p>
                        <p style="margin:0 0 28px;font-size:14px;color:#6B7280;line-height:1.6">
                          Tài khoản EduChatbot của bạn đã được tạo thành công bởi quản trị viên.
                          Dưới đây là thông tin đăng nhập của bạn:
                        </p>

                        <!-- Credentials box -->
                        <table width="100%" cellpadding="0" cellspacing="0"
                               style="background:#F8FAFF;border:1px solid #E0E7FF;border-radius:12px;margin-bottom:28px">
                          <tr>
                            <td style="padding:24px 28px">
                              <table width="100%" cellpadding="0" cellspacing="0">
                                <tr>
                                  <td style="padding-bottom:16px">
                                    <span style="display:block;font-size:11px;font-weight:600;color:#6366F1;text-transform:uppercase;letter-spacing:0.5px;margin-bottom:6px">Tên đăng nhập</span>
                                    <span style="display:block;font-size:18px;font-weight:700;color:#111827;font-family:monospace;background:#EEF2FF;padding:8px 14px;border-radius:8px;letter-spacing:0.5px">{username}</span>
                                  </td>
                                </tr>
                                <tr>
                                  <td>
                                    <span style="display:block;font-size:11px;font-weight:600;color:#6366F1;text-transform:uppercase;letter-spacing:0.5px;margin-bottom:6px">Mật khẩu</span>
                                    <span style="display:block;font-size:18px;font-weight:700;color:#111827;font-family:monospace;background:#EEF2FF;padding:8px 14px;border-radius:8px;letter-spacing:0.5px">{password}</span>
                                  </td>
                                </tr>
                              </table>
                            </td>
                          </tr>
                        </table>

                        <!-- Security notice -->
                        <table width="100%" cellpadding="0" cellspacing="0"
                               style="background:#FFF7ED;border:1px solid #FED7AA;border-radius:10px;margin-bottom:28px">
                          <tr>
                            <td style="padding:14px 18px">
                              <p style="margin:0;font-size:13px;color:#92400E;line-height:1.6">
                                ⚠️ <strong>Lưu ý bảo mật:</strong> Vui lòng đổi mật khẩu sau lần đăng nhập đầu tiên
                                và không chia sẻ thông tin này với người khác.
                              </p>
                            </td>
                          </tr>
                        </table>

                        <p style="margin:0;font-size:13px;color:#9CA3AF;line-height:1.7">
                          Nếu bạn có bất kỳ thắc mắc nào, hãy trả lời email này để liên hệ với quản trị viên.<br>
                          Trân trọng,<br>
                          <strong style="color:#6366F1">{adminName}</strong>
                          {(string.IsNullOrWhiteSpace(adminEmail) ? "" : $"<br><span style='font-size:12px;color:#6366F1'>{adminEmail}</span>")}
                        </p>
                      </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                      <td style="background:#F8FAFF;padding:20px 40px;text-align:center;border-top:1px solid #E5E7EB">
                        <p style="margin:0;font-size:11px;color:#9CA3AF">
                          Email này được gửi tự động từ hệ thống EduChatbot — PRN222 Assignment · FPT University
                        </p>
                      </td>
                    </tr>

                  </table>
                </td></tr>
              </table>
            </body>
            </html>
            """;
        }
    }
}

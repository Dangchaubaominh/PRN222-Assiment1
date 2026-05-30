using System;
using System.Security.Cryptography;
using System.Text;
using RagChatbot.BLL.Services.Interfaces;
using RagChatbot.DAL.Entities;
using RagChatbot.DAL.Repositories.Interfaces;

namespace RagChatbot.BLL.Services.Implements
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;

        public AccountService(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public Account? Authenticate(string email, string password)
        {
            var account = _accountRepository.GetByEmail(email);
            if (account == null) return null;

            // Verify password using SHA256 hash
            string hashedPassword = HashPassword(password);
            if (account.Password == hashedPassword)
            {
                return account;
            }
            return null;
        }

        public bool Register(string email, string password, string fullName, string role)
        {
            var existing = _accountRepository.GetByEmail(email);
            if (existing != null) return false;

            var account = new Account
            {
                Id = Guid.NewGuid(),
                Email = email,
                Password = HashPassword(password),
                FullName = fullName,
                Role = role,
                CreatedAt = DateTime.UtcNow
            };

            _accountRepository.Add(account);
            return true;
        }

        public bool SeedAdminAccount()
        {
            // Seed matching the default database connection username
            string defaultEmail = "admin@FUNewsManagementSystem.org";
            var existing = _accountRepository.GetByEmail(defaultEmail);
            if (existing == null)
            {
                return Register(defaultEmail, "@@abc123@@", "Senior FullStack Admin", "Admin");
            }
            return false;
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var sb = new StringBuilder();
                foreach (var b in bytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}

using RagChatbot.DAL.Data;
using RagChatbot.DAL.Entities;
using RagChatbot.DAL.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace RagChatbot.DAL.Repositories.Implements
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public User GetUserByCredentials(string username, string password)
        {
            return _context.Users.FirstOrDefault(u => u.Username == username && u.Password == password);
        }

        public User GetByUsername(string username)
        {
            return _context.Users.FirstOrDefault(u => u.Username == username);
        }

        public User GetByEmail(string email)
        {
            return _context.Users.FirstOrDefault(u => u.Email == email);
        }

        public User GetByResetToken(string token)
        {
            return _context.Users.FirstOrDefault(u => u.PasswordResetToken == token);
        }

        public void SetResetToken(int userId, string token, DateTime expiry)
        {
            var user = _context.Users.Find(userId);
            if (user == null) return;
            user.PasswordResetToken  = token;
            user.PasswordResetExpiry = expiry;
            _context.SaveChanges();
        }

        public void UpdatePassword(int userId, string newPassword)
        {
            var user = _context.Users.Find(userId);
            if (user == null) return;
            user.Password            = newPassword;
            user.PasswordResetToken  = null;
            user.PasswordResetExpiry = null;
            _context.SaveChanges();
        }

        public IEnumerable<User> GetAll()
        {
            return _context.Users.OrderBy(u => u.Id).ToList();
        }

        public User GetById(int id)
        {
            return _context.Users.FirstOrDefault(u => u.Id == id);
        }

        public void Add(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public void Update(User user)
        {
            var existing = _context.Users.Find(user.Id);
            if (existing == null) return;
            existing.FullName = user.FullName;
            existing.Email    = user.Email;
            existing.Role     = user.Role;
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var user = _context.Users.Find(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
        }
    }
}

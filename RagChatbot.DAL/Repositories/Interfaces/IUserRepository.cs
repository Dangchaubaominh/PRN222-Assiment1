using RagChatbot.DAL.Entities;
using System.Collections.Generic;

namespace RagChatbot.DAL.Repositories.Interfaces
{
    public interface IUserRepository
    {
        User GetUserByCredentials(string username, string password);
        User GetByUsername(string username);
        User GetByEmail(string email);
        User GetByResetToken(string token);
        IEnumerable<User> GetAll();
        User GetById(int id);
        void Add(User user);
        void Update(User user);
        void Delete(int id);
        void SetResetToken(int userId, string token, DateTime expiry);
        void UpdatePassword(int userId, string newPassword);
    }
}

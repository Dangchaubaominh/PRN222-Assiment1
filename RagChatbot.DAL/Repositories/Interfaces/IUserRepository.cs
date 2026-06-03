using RagChatbot.DAL.Entities;
using System.Collections.Generic;

namespace RagChatbot.DAL.Repositories.Interfaces
{
    public interface IUserRepository
    {
        User GetUserByCredentials(string username, string password);
        IEnumerable<User> GetAll();
        User GetById(int id);
        void Add(User user);
        void Delete(int id);
    }
}

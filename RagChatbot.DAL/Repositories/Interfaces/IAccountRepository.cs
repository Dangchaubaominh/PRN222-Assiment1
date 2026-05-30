using System;
using System.Collections.Generic;
using RagChatbot.DAL.Entities;

namespace RagChatbot.DAL.Repositories.Interfaces
{
    public interface IAccountRepository
    {
        Account? GetById(Guid id);
        Account? GetByEmail(string email);
        void Add(Account account);
        void Update(Account account);
        void Delete(Guid id);
    }
}

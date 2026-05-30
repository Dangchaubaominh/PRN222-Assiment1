using System;
using RagChatbot.DAL.Entities;

namespace RagChatbot.BLL.Services.Interfaces
{
    public interface IAccountService
    {
        Account? Authenticate(string email, string password);
        bool Register(string email, string password, string fullName, string role);
        bool SeedAdminAccount();
    }
}

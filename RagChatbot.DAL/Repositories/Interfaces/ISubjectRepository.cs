using System;
using System.Collections.Generic;
using RagChatbot.DAL.Entities;

namespace RagChatbot.DAL.Repositories.Interfaces
{
    public interface ISubjectRepository
    {
        // R (Read) - Sử dụng LINQ để lấy danh sách và tìm kiếm
        IEnumerable<Subject> GetAll();
        IEnumerable<Subject> SearchByName(string keyword);
        Subject GetById(Guid id);

        // C, U, D (Create, Update, Delete)
        void Add(Subject subject);
        void Update(Subject subject);
        void Delete(Guid id);
    }
}
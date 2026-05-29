using System;
using System.Collections.Generic;
using RagChatbot.DAL.Entities;

namespace RagChatbot.BLL.Services.Interfaces
{
    public interface ISubjectService
    {
        IEnumerable<Subject> GetAllSubjects();
        IEnumerable<Subject> SearchSubjects(string keyword);
        Subject GetSubjectById(Guid id);
        bool CreateSubject(Subject subject);
        bool UpdateSubject(Subject subject);
        bool DeleteSubject(Guid id);
    }
}
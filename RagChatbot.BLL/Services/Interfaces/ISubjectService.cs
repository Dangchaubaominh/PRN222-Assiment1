using RagChatbot.BLL.DTOs;
using System;
using System.Collections.Generic;

namespace RagChatbot.BLL.Services.Interfaces
{
    public interface ISubjectService
    {
        IEnumerable<SubjectDto> GetAllSubjects();
        IEnumerable<SubjectDto> SearchSubjects(string keyword);
        SubjectDto GetSubjectById(Guid id);
        bool CreateSubject(SubjectDto subjectDto);
        bool UpdateSubject(SubjectDto subjectDto);
        bool DeleteSubject(Guid id);
    }
}

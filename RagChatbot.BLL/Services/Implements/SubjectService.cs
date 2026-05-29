using System;
using System.Collections.Generic;
using RagChatbot.BLL.Services.Interfaces;
using RagChatbot.DAL.Entities;
using RagChatbot.DAL.Repositories.Interfaces;

namespace RagChatbot.BLL.Services.Implements
{
    public class SubjectService : ISubjectService
    {
        private readonly ISubjectRepository _subjectRepository;

        // Dependency Injection: Gọi tầng DAL (Repository) vào tầng BLL
        public SubjectService(ISubjectRepository subjectRepository)
        {
            _subjectRepository = subjectRepository;
        }

        public IEnumerable<Subject> GetAllSubjects()
        {
            return _subjectRepository.GetAll();
        }

        public IEnumerable<Subject> SearchSubjects(string keyword)
        {
            return _subjectRepository.SearchByName(keyword);
        }

        public Subject GetSubjectById(Guid id)
        {
            return _subjectRepository.GetById(id);
        }

        public bool CreateSubject(Subject subject)
        {
            // Logic nghiệp vụ: Ví dụ kiểm tra tên không được chứa ký tự cấm (nếu cần)
            if (string.IsNullOrWhiteSpace(subject.Name) || string.IsNullOrWhiteSpace(subject.Code))
                return false;

            subject.Id = Guid.NewGuid();
            subject.CreatedAt = DateTime.UtcNow;

            _subjectRepository.Add(subject);
            return true;
        }

        public bool UpdateSubject(Subject subject)
        {
            var existingSubject = _subjectRepository.GetById(subject.Id);
            if (existingSubject == null) return false;

            existingSubject.Code = subject.Code;
            existingSubject.Name = subject.Name;

            _subjectRepository.Update(existingSubject);
            return true;
        }

        public bool DeleteSubject(Guid id)
        {
            if (_subjectRepository.GetById(id) == null) return false;

            _subjectRepository.Delete(id);
            return true;
        }
    }
}
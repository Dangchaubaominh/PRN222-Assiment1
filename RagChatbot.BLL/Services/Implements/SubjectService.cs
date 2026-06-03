using RagChatbot.BLL.DTOs;
using RagChatbot.BLL.Services.Interfaces;
using RagChatbot.DAL.Entities;
using RagChatbot.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RagChatbot.BLL.Services.Implements
{
    public class SubjectService : ISubjectService
    {
        private readonly ISubjectRepository _subjectRepository;

        public SubjectService(ISubjectRepository subjectRepository)
        {
            _subjectRepository = subjectRepository;
        }

        public IEnumerable<SubjectDto> GetAllSubjects()
        {
            return _subjectRepository.GetAll().Select(ToDto);
        }

        public IEnumerable<SubjectDto> SearchSubjects(string keyword)
        {
            return _subjectRepository.SearchByName(keyword).Select(ToDto);
        }

        public SubjectDto GetSubjectById(Guid id)
        {
            var entity = _subjectRepository.GetById(id);
            return entity == null ? null : ToDto(entity);
        }

        public bool CreateSubject(SubjectDto subjectDto)
        {
            var entity = new Subject
            {
                Id = Guid.NewGuid(),
                Code = subjectDto.Code,
                Name = subjectDto.Name,
                CreatedAt = DateTime.UtcNow
            };
            _subjectRepository.Add(entity);
            return true;
        }

        public bool UpdateSubject(SubjectDto subjectDto)
        {
            var existing = _subjectRepository.GetById(subjectDto.Id);
            if (existing == null) return false;

            existing.Code = subjectDto.Code;
            existing.Name = subjectDto.Name;
            _subjectRepository.Update(existing);
            return true;
        }

        public bool DeleteSubject(Guid id)
        {
            if (_subjectRepository.GetById(id) == null) return false;
            _subjectRepository.Delete(id);
            return true;
        }

        private static SubjectDto ToDto(Subject s) => new SubjectDto
        {
            Id = s.Id,
            Code = s.Code,
            Name = s.Name,
            CreatedAt = s.CreatedAt
        };
    }
}

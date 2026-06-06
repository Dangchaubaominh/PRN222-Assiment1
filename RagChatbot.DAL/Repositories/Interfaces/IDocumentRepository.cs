using System;
using System.Collections.Generic;
using RagChatbot.DAL.Entities;

namespace RagChatbot.DAL.Repositories.Interfaces
{
    public interface IDocumentRepository
    {
        // Lấy danh sách tài liệu theo Mã môn học
        IEnumerable<Document> GetDocumentsBySubjectId(Guid subjectId);

        Document GetById(Guid id);

        // Thêm và Xóa
        void Add(Document document);
        void Delete(Guid id);

        // Cập nhật trạng thái (Dùng cho lúc Chatbot đang xử lý file)
        void UpdateStatus(Guid id, DocumentStatus newStatus);

        bool ExistsByFileName(Guid subjectId, string fileName);
    }
}
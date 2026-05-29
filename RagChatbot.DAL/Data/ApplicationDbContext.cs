using RagChatbot.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace RagChatbot.DAL.Data
{
    public class ApplicationDbContext : DbContext
    {
        // Constructor nhận cấu hình từ lớp MVC truyền xuống
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Khai báo các bảng sẽ có trong Database
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Document> Documents { get; set; }

        // (Sau này làm Assignment 2 mình sẽ thêm DbSet cho bảng ChatSession, ChatMessage vào đây)

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Nếu bạn có các cấu hình nâng cao không dùng Data Annotations thì viết ở đây
            // Hiện tại mình đã dùng Data Annotations [Table], [Key] ở các file Entities nên chỗ này có thể để trống.
        }
    }
}
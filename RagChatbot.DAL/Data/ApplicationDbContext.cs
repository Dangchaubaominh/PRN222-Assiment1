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
        public DbSet<DocumentChunk> DocumentChunks { get; set; }
        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasPostgresExtension("vector");
        }
    }
}
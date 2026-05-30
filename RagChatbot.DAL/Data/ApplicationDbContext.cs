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
        public DbSet<Account> Accounts { get; set; }
        public DbSet<DocumentChunk> DocumentChunks { get; set; }
        public DbSet<ChatSession> ChatSessions { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<BenchmarkResult> BenchmarkResults { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<DocumentChunk>()
                .HasOne(dc => dc.Document)
                .WithMany()
                .HasForeignKey(dc => dc.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChatSession>()
                .HasOne(cs => cs.Subject)
                .WithMany()
                .HasForeignKey(cs => cs.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChatSession>()
                .HasOne(cs => cs.Account)
                .WithMany()
                .HasForeignKey(cs => cs.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChatMessage>()
                .HasOne(cm => cm.Session)
                .WithMany(cs => cs.Messages)
                .HasForeignKey(cm => cm.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
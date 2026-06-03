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
        public DbSet<User> Users { get; set; }
        public DbSet<UserSubject> UserSubjects { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasPostgresExtension("vector");

            modelBuilder.Entity<User>().ToTable("User");

            modelBuilder.Entity<UserSubject>().HasKey(us => new { us.UserId, us.SubjectId });

            modelBuilder.Entity<UserSubject>()
                .HasOne(us => us.User)
                .WithMany(u => u.UserSubjects)
                .HasForeignKey(us => us.UserId);

            modelBuilder.Entity<UserSubject>()
                .HasOne(us => us.Subject)
                .WithMany(s => s.UserSubjects)
                .HasForeignKey(us => us.SubjectId);

            modelBuilder.Entity<User>().HasData(
                new User { Id = 1,  Username = "admin",       Password = "123", Role = "Admin",    FullName = "Nguyễn Quản Trị" },
                new User { Id = 2,  Username = "giangvien",   Password = "123", Role = "Lecturer", FullName = "Trần Thị Hương" },
                new User { Id = 3,  Username = "sinhvien",    Password = "123", Role = "Student",  FullName = "Lê Văn An" },
                new User { Id = 4,  Username = "gv_minh",     Password = "123", Role = "Lecturer", FullName = "Phạm Quốc Minh" },
                new User { Id = 5,  Username = "gv_lan",      Password = "123", Role = "Lecturer", FullName = "Ngô Thị Lan" },
                new User { Id = 6,  Username = "sv_bao",      Password = "123", Role = "Student",  FullName = "Đặng Châu Bảo" },
                new User { Id = 7,  Username = "sv_tung",     Password = "123", Role = "Student",  FullName = "Hoàng Minh Tùng" },
                new User { Id = 8,  Username = "sv_linh",     Password = "123", Role = "Student",  FullName = "Vũ Thị Linh" },
                new User { Id = 9,  Username = "sv_khoa",     Password = "123", Role = "Student",  FullName = "Bùi Thanh Khoa" },
                new User { Id = 10, Username = "sv_ngan",     Password = "123", Role = "Student",  FullName = "Trịnh Thị Ngân" },
                new User { Id = 11, Username = "sv_hieu",     Password = "123", Role = "Student",  FullName = "Lý Công Hiếu" },
                new User { Id = 12, Username = "sv_phuong",   Password = "123", Role = "Student",  FullName = "Dương Thị Phương" },
                new User { Id = 13, Username = "sv_duc",      Password = "123", Role = "Student",  FullName = "Mai Xuân Đức" }
            );
        }
    }
}
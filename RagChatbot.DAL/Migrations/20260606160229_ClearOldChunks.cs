using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RagChatbot.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ClearOldChunks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Xóa toàn bộ chunk cũ (được chia bởi TextChunker — cắt cứng theo từ)
            migrationBuilder.Sql(@"TRUNCATE TABLE ""DocumentChunks"";");

            // Reset trạng thái tài liệu về Pending (0) để upload lại sẽ dùng SemanticChunker
            migrationBuilder.Sql(@"UPDATE ""Documents"" SET ""Status"" = 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Không thể khôi phục dữ liệu chunk đã xóa
        }
    }
}

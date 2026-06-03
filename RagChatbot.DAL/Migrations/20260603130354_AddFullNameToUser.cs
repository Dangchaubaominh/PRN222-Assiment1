using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RagChatbot.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddFullNameToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "User",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 1,
                column: "FullName",
                value: "Nguyễn Quản Trị");

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 2,
                column: "FullName",
                value: "Trần Thị Hương");

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 3,
                column: "FullName",
                value: "Lê Văn An");

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "FullName", "Password", "Role", "Username" },
                values: new object[,]
                {
                    { 4, "Phạm Quốc Minh", "123", "Lecturer", "gv_minh" },
                    { 5, "Ngô Thị Lan", "123", "Lecturer", "gv_lan" },
                    { 6, "Đặng Châu Bảo", "123", "Student", "sv_bao" },
                    { 7, "Hoàng Minh Tùng", "123", "Student", "sv_tung" },
                    { 8, "Vũ Thị Linh", "123", "Student", "sv_linh" },
                    { 9, "Bùi Thanh Khoa", "123", "Student", "sv_khoa" },
                    { 10, "Trịnh Thị Ngân", "123", "Student", "sv_ngan" },
                    { 11, "Lý Công Hiếu", "123", "Student", "sv_hieu" },
                    { 12, "Dương Thị Phương", "123", "Student", "sv_phuong" },
                    { 13, "Mai Xuân Đức", "123", "Student", "sv_duc" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "User",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "User",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "User",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "User",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "User",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "User",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "User",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "User",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "User",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "User",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "User");
        }
    }
}

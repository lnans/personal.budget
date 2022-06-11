using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    public partial class banknameunique : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Accounts_Name_OwnerId",
                table: "Accounts");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Name_Bank_OwnerId",
                table: "Accounts",
                columns: new[] { "Name", "Bank", "OwnerId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Accounts_Name_Bank_OwnerId",
                table: "Accounts");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Name_OwnerId",
                table: "Accounts",
                columns: new[] { "Name", "OwnerId" },
                unique: true);
        }
    }
}

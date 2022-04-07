using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    public partial class operationtype : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsIncome",
                table: "Operations");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Operations",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Operations");

            migrationBuilder.AddColumn<bool>(
                name: "IsIncome",
                table: "Operations",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}

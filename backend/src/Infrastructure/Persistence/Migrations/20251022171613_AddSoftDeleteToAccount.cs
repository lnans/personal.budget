using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true
            );

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Accounts",
                type: "timestamp with time zone",
                nullable: true
            );

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "AccountOperations",
                type: "timestamp with time zone",
                nullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "DeletedAt", table: "Users");

            migrationBuilder.DropColumn(name: "DeletedAt", table: "Accounts");

            migrationBuilder.DropColumn(name: "DeletedAt", table: "AccountOperations");
        }
    }
}

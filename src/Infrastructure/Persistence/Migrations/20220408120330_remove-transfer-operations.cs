using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    public partial class removetransferoperations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Operations_Accounts_TransferAccountId",
                table: "Operations");

            migrationBuilder.DropForeignKey(
                name: "FK_Operations_Users_CreatedById",
                table: "Operations");

            migrationBuilder.DropIndex(
                name: "IX_Operations_TransferAccountId",
                table: "Operations");

            migrationBuilder.RenameColumn(
                name: "TransferAccountId",
                table: "Operations",
                newName: "ExecutionDate");

            migrationBuilder.RenameColumn(
                name: "OperationDate",
                table: "Operations",
                newName: "CreationDate");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedById",
                table: "Operations",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Operations_Users_CreatedById",
                table: "Operations",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Operations_Users_CreatedById",
                table: "Operations");

            migrationBuilder.RenameColumn(
                name: "ExecutionDate",
                table: "Operations",
                newName: "TransferAccountId");

            migrationBuilder.RenameColumn(
                name: "CreationDate",
                table: "Operations",
                newName: "OperationDate");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedById",
                table: "Operations",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_TransferAccountId",
                table: "Operations",
                column: "TransferAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Operations_Accounts_TransferAccountId",
                table: "Operations",
                column: "TransferAccountId",
                principalTable: "Accounts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Operations_Users_CreatedById",
                table: "Operations",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}

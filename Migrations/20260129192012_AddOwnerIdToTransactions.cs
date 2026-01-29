using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Expense_Tracker_mvc.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerIdToTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Transactions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "TransactionCategories",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionCategories_OwnerId",
                table: "TransactionCategories",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionCategories_AspNetUsers_OwnerId",
                table: "TransactionCategories",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransactionCategories_AspNetUsers_OwnerId",
                table: "TransactionCategories");

            migrationBuilder.DropIndex(
                name: "IX_TransactionCategories_OwnerId",
                table: "TransactionCategories");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "TransactionCategories");
        }
    }
}

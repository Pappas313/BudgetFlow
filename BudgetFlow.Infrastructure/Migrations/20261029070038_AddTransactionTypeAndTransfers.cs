using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionTypeAndTransfers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "TransactionCategoryId",
                table: "FinancialTransactions",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "ExternalReference",
                table: "FinancialTransactions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TransactionType",
                table: "FinancialTransactions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "TransferGroupId",
                table: "FinancialTransactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_BankAccountId_ExternalReference",
                table: "FinancialTransactions",
                columns: new[] { "BankAccountId", "ExternalReference" },
                unique: true,
                filter: "[ExternalReference] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_TransferGroupId",
                table: "FinancialTransactions",
                column: "TransferGroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FinancialTransactions_BankAccountId_ExternalReference",
                table: "FinancialTransactions");

            migrationBuilder.DropIndex(
                name: "IX_FinancialTransactions_TransferGroupId",
                table: "FinancialTransactions");

            migrationBuilder.DropColumn(
                name: "ExternalReference",
                table: "FinancialTransactions");

            migrationBuilder.DropColumn(
                name: "TransactionType",
                table: "FinancialTransactions");

            migrationBuilder.DropColumn(
                name: "TransferGroupId",
                table: "FinancialTransactions");

            migrationBuilder.AlterColumn<Guid>(
                name: "TransactionCategoryId",
                table: "FinancialTransactions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}

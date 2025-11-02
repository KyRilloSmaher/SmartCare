using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCare.InfraStructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Order_Store_StoreId",
                table: "Order");

            migrationBuilder.RenameColumn(
                name: "TransactionId",
                table: "Payment",
                newName: "SessionId");

            migrationBuilder.RenameIndex(
                name: "IX_Payment_TransactionId",
                table: "Payment",
                newName: "IX_Payment_SessionId");

            migrationBuilder.AddColumn<string>(
                name: "PaymentIntentId",
                table: "Payment",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "StoreId",
                table: "Order",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_PaymentIntentId",
                table: "Payment",
                column: "PaymentIntentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Store_StoreId",
                table: "Order",
                column: "StoreId",
                principalTable: "Store",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Order_Store_StoreId",
                table: "Order");

            migrationBuilder.DropIndex(
                name: "IX_Payment_PaymentIntentId",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "PaymentIntentId",
                table: "Payment");

            migrationBuilder.RenameColumn(
                name: "SessionId",
                table: "Payment",
                newName: "TransactionId");

            migrationBuilder.RenameIndex(
                name: "IX_Payment_SessionId",
                table: "Payment",
                newName: "IX_Payment_TransactionId");

            migrationBuilder.AlterColumn<Guid>(
                name: "StoreId",
                table: "Order",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Store_StoreId",
                table: "Order",
                column: "StoreId",
                principalTable: "Store",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

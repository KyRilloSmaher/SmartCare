using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCare.InfraStructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePaymentandOrderRelationShip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payment_SessionId",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "url",
                table: "Payment");

            migrationBuilder.RenameColumn(
                name: "PaymentMethod",
                table: "Payment",
                newName: "Version");

            migrationBuilder.RenameColumn(
                name: "ExpiredAt",
                table: "Payment",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "PaymentId",
                table: "Order",
                newName: "PaymentVersion");

            migrationBuilder.AddColumn<string>(
                name: "ClientSecret",
                table: "Payment",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Method",
                table: "Payment",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "PaymentIntentId",
                table: "Order",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientSecret",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "Method",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "PaymentIntentId",
                table: "Order");

            migrationBuilder.RenameColumn(
                name: "Version",
                table: "Payment",
                newName: "PaymentMethod");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Payment",
                newName: "ExpiredAt");

            migrationBuilder.RenameColumn(
                name: "PaymentVersion",
                table: "Order",
                newName: "PaymentId");

            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                table: "Payment",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "url",
                table: "Payment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payment_SessionId",
                table: "Payment",
                column: "SessionId");
        }
    }
}

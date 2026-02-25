using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCare.InfraStructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsUsed",
                table: "EmailVerifications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UsedAt",
                table: "EmailVerifications",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_InventoryId",
                table: "Reservation",
                column: "InventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_ProductId",
                table: "Reservation",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservation_Inventory_InventoryId",
                table: "Reservation",
                column: "InventoryId",
                principalTable: "Inventory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservation_Products_ProductId",
                table: "Reservation",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservation_Inventory_InventoryId",
                table: "Reservation");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservation_Products_ProductId",
                table: "Reservation");

            migrationBuilder.DropIndex(
                name: "IX_Reservation_InventoryId",
                table: "Reservation");

            migrationBuilder.DropIndex(
                name: "IX_Reservation_ProductId",
                table: "Reservation");

            migrationBuilder.DropColumn(
                name: "IsUsed",
                table: "EmailVerifications");

            migrationBuilder.DropColumn(
                name: "UsedAt",
                table: "EmailVerifications");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCare.InfraStructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateorderFunctionalities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItem_Reservation_ReservationId",
                table: "OrderItem");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservation_CartItem_CartItemId",
                table: "Reservation");

            migrationBuilder.DropIndex(
                name: "IX_Reservation_CartItemId",
                table: "Reservation");

            migrationBuilder.DropIndex(
                name: "IX_OrderItem_ReservationId",
                table: "OrderItem");

            migrationBuilder.DropIndex(
                name: "IX_CartItem_ReservationId",
                table: "CartItem");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_Code",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ReservationId",
                table: "CartItem");

            migrationBuilder.RenameColumn(
                name: "CartItemId",
                table: "Reservation",
                newName: "ProductId");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "AspNetUsers",
                newName: "OTP");

            migrationBuilder.AddColumn<Guid>(
                name: "InventoryId",
                table: "Reservation",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "OrderItemId",
                table: "Reservation",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ReservationId",
                table: "OrderItem",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "PickupCodeHash",
                table: "Order",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailConfirmationLink",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "VerificationURLExpiresAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_OrderItemId",
                table: "Reservation",
                column: "OrderItemId",
                unique: true,
                filter: "[OrderItemId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Order_PickupCodeHash",
                table: "Order",
                column: "PickupCodeHash");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_OTP",
                table: "AspNetUsers",
                column: "OTP",
                unique: true,
                filter: "[OTP] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservation_OrderItem_OrderItemId",
                table: "Reservation",
                column: "OrderItemId",
                principalTable: "OrderItem",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservation_OrderItem_OrderItemId",
                table: "Reservation");

            migrationBuilder.DropIndex(
                name: "IX_Reservation_OrderItemId",
                table: "Reservation");

            migrationBuilder.DropIndex(
                name: "IX_Order_PickupCodeHash",
                table: "Order");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_OTP",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "InventoryId",
                table: "Reservation");

            migrationBuilder.DropColumn(
                name: "OrderItemId",
                table: "Reservation");

            migrationBuilder.DropColumn(
                name: "PickupCodeHash",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "EmailConfirmationLink",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "VerificationURLExpiresAt",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "Reservation",
                newName: "CartItemId");

            migrationBuilder.RenameColumn(
                name: "OTP",
                table: "AspNetUsers",
                newName: "Code");

            migrationBuilder.AlterColumn<Guid>(
                name: "ReservationId",
                table: "OrderItem",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReservationId",
                table: "CartItem",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_CartItemId",
                table: "Reservation",
                column: "CartItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItem_ReservationId",
                table: "OrderItem",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItem_ReservationId",
                table: "CartItem",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Code",
                table: "AspNetUsers",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItem_Reservation_ReservationId",
                table: "OrderItem",
                column: "ReservationId",
                principalTable: "Reservation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservation_CartItem_CartItemId",
                table: "Reservation",
                column: "CartItemId",
                principalTable: "CartItem",
                principalColumn: "CartItemId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

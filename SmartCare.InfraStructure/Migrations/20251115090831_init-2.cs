using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCare.InfraStructure.Migrations
{
    /// <inheritdoc />
    public partial class init2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OnlineOrders_UserAddress_AddressId",
                table: "OnlineOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_OnlineOrders_UserAddress_AddressId1",
                table: "OnlineOrders");

            migrationBuilder.DropIndex(
                name: "IX_OnlineOrders_AddressId1",
                table: "OnlineOrders");

            migrationBuilder.DropColumn(
                name: "AddressId1",
                table: "OnlineOrders");

            migrationBuilder.AlterColumn<Guid>(
                name: "AddressId",
                table: "OnlineOrders",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "ShippingAddressId",
                table: "OnlineOrders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_OnlineOrders_ShippingAddressId",
                table: "OnlineOrders",
                column: "ShippingAddressId");

            migrationBuilder.AddForeignKey(
                name: "FK_OnlineOrders_UserAddress_AddressId",
                table: "OnlineOrders",
                column: "AddressId",
                principalTable: "UserAddress",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OnlineOrders_UserAddress_ShippingAddressId",
                table: "OnlineOrders",
                column: "ShippingAddressId",
                principalTable: "UserAddress",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OnlineOrders_UserAddress_AddressId",
                table: "OnlineOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_OnlineOrders_UserAddress_ShippingAddressId",
                table: "OnlineOrders");

            migrationBuilder.DropIndex(
                name: "IX_OnlineOrders_ShippingAddressId",
                table: "OnlineOrders");

            migrationBuilder.DropColumn(
                name: "ShippingAddressId",
                table: "OnlineOrders");

            migrationBuilder.AlterColumn<Guid>(
                name: "AddressId",
                table: "OnlineOrders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AddressId1",
                table: "OnlineOrders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OnlineOrders_AddressId1",
                table: "OnlineOrders",
                column: "AddressId1");

            migrationBuilder.AddForeignKey(
                name: "FK_OnlineOrders_UserAddress_AddressId",
                table: "OnlineOrders",
                column: "AddressId",
                principalTable: "UserAddress",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OnlineOrders_UserAddress_AddressId1",
                table: "OnlineOrders",
                column: "AddressId1",
                principalTable: "UserAddress",
                principalColumn: "Id");
        }
    }
}

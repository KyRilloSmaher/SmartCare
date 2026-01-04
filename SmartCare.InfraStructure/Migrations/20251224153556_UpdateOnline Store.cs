using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCare.InfraStructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOnlineStore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OnlineOrders_UserAddress_AddressId",
                table: "OnlineOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_OnlineOrders_UserAddress_ShippingAddressId",
                table: "OnlineOrders");

            migrationBuilder.DropIndex(
                name: "IX_OnlineOrders_AddressId",
                table: "OnlineOrders");

            migrationBuilder.DropColumn(
                name: "AddressId",
                table: "OnlineOrders");

            migrationBuilder.AddForeignKey(
                name: "FK_OnlineOrders_UserAddress_ShippingAddressId",
                table: "OnlineOrders",
                column: "ShippingAddressId",
                principalTable: "UserAddress",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OnlineOrders_UserAddress_ShippingAddressId",
                table: "OnlineOrders");

            migrationBuilder.AddColumn<Guid>(
                name: "AddressId",
                table: "OnlineOrders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OnlineOrders_AddressId",
                table: "OnlineOrders",
                column: "AddressId");

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
                onDelete: ReferentialAction.Cascade);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCare.InfraStructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFromStoreOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Order_PickupCodeHash",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "PickupCodeHash",
                table: "Order");

            migrationBuilder.AddColumn<string>(
                name: "PickupCodeHash",
                table: "FromStoreOrders",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FromStoreOrders_PickupCodeHash",
                table: "FromStoreOrders",
                column: "PickupCodeHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FromStoreOrders_PickupCodeHash",
                table: "FromStoreOrders");

            migrationBuilder.DropColumn(
                name: "PickupCodeHash",
                table: "FromStoreOrders");

            migrationBuilder.AddColumn<string>(
                name: "PickupCodeHash",
                table: "Order",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Order_PickupCodeHash",
                table: "Order",
                column: "PickupCodeHash");
        }
    }
}

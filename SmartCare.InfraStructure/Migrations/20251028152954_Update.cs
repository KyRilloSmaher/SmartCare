using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCare.InfraStructure.Migrations
{
    /// <inheritdoc />
    public partial class Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Favorite_ProductId",
                table: "Favorite");

            migrationBuilder.AddColumn<int>(
                name: "ProductsCount",
                table: "Company",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Rate_ProductId_ClientId",
                table: "Rate",
                columns: new[] { "ProductId", "ClientId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Favorite_ProductId_ClientId",
                table: "Favorite",
                columns: new[] { "ProductId", "ClientId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Rate_ProductId_ClientId",
                table: "Rate");

            migrationBuilder.DropIndex(
                name: "IX_Favorite_ProductId_ClientId",
                table: "Favorite");

            migrationBuilder.DropColumn(
                name: "ProductsCount",
                table: "Company");

            migrationBuilder.CreateIndex(
                name: "IX_Favorite_ProductId",
                table: "Favorite",
                column: "ProductId");
        }
    }
}

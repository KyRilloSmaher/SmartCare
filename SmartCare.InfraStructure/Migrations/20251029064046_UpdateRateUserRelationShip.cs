using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCare.InfraStructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRateUserRelationShip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the existing FK that cascades deletes
            migrationBuilder.DropForeignKey(
                name: "FK_Rate_AspNetUsers_ClientId",
                table: "Rate");

            // Recreate the FK with SET NULL delete behavior
            migrationBuilder.AddForeignKey(
                name: "FK_Rate_AspNetUsers_ClientId",
                table: "Rate",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert to cascade behavior
            migrationBuilder.DropForeignKey(
                name: "FK_Rate_AspNetUsers_ClientId",
                table: "Rate");

            migrationBuilder.AddForeignKey(
                name: "FK_Rate_AspNetUsers_ClientId",
                table: "Rate",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }



    }
}

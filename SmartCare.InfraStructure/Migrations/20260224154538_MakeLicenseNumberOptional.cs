using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCare.InfraStructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeLicenseNumberOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_pharmacists_LicenseNumber",
                table: "pharmacists");

            migrationBuilder.AlterColumn<string>(
                name: "LicenseNumber",
                table: "pharmacists",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.CreateIndex(
                name: "IX_pharmacists_LicenseNumber",
                table: "pharmacists",
                column: "LicenseNumber",
                unique: true,
                filter: "[LicenseNumber] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_pharmacists_LicenseNumber",
                table: "pharmacists");

            migrationBuilder.AlterColumn<string>(
                name: "LicenseNumber",
                table: "pharmacists",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_pharmacists_LicenseNumber",
                table: "pharmacists",
                column: "LicenseNumber",
                unique: true);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCare.InfraStructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateASPNETUsersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailConfirmationLink",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "VerificationURLExpiresAt",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "address",
                table: "UserAddress",
                newName: "AddressLine");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "UserAddress",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Order",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OTPAttempts",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "OTPExpiryTime",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ResetPasswordConfirmed",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "EmailVerifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailVerifications", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailVerifications");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "UserAddress");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "OTPAttempts",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "OTPExpiryTime",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ResetPasswordConfirmed",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "AddressLine",
                table: "UserAddress",
                newName: "address");

            migrationBuilder.AddColumn<string>(
                name: "EmailConfirmationLink",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerificationURLExpiresAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}

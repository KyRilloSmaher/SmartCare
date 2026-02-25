using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCare.InfraStructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUsersTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cart_AspNetUsers_ClientId",
                table: "Cart");

            migrationBuilder.DropForeignKey(
                name: "FK_Favorite_AspNetUsers_ClientId",
                table: "Favorite");

            migrationBuilder.DropForeignKey(
                name: "FK_Order_AspNetUsers_ClientId",
                table: "Order");

            migrationBuilder.DropForeignKey(
                name: "FK_pharmacists_Store_StoreId",
                table: "pharmacists");

            migrationBuilder.DropForeignKey(
                name: "FK_Rate_AspNetUsers_ClientId",
                table: "Rate");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAddress_AspNetUsers_ClientId",
                table: "UserAddress");

            migrationBuilder.DropPrimaryKey(
                name: "PK_pharmacists",
                table: "pharmacists");

            migrationBuilder.DropIndex(
                name: "IX_pharmacists_LicenseNumber",
                table: "pharmacists");

            migrationBuilder.DropIndex(
                name: "IX_pharmacists_OTP",
                table: "pharmacists");

            migrationBuilder.DropColumn(
                name: "AccessFailedCount",
                table: "pharmacists");

            migrationBuilder.DropColumn(
                name: "BirthDate",
                table: "pharmacists");

            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "pharmacists");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "pharmacists");

            migrationBuilder.DropColumn(
                name: "EmailConfirmationLink",
                table: "pharmacists");

            migrationBuilder.DropColumn(
                name: "EmailConfirmed",
                table: "pharmacists");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "pharmacists");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "pharmacists");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "pharmacists");

            migrationBuilder.DropColumn(
                name: "LockoutEnabled",
                table: "pharmacists");

            migrationBuilder.DropColumn(
                name: "LockoutEnd",
                table: "pharmacists");

            migrationBuilder.DropColumn(
                name: "NormalizedEmail",
                table: "pharmacists");

            migrationBuilder.DropColumn(
                name: "NormalizedUserName",
                table: "pharmacists");

            migrationBuilder.DropColumn(
                name: "OTP",
                table: "pharmacists");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "pharmacists");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "pharmacists");

            migrationBuilder.DropColumn(
                name: "PhoneNumberConfirmed",
                table: "pharmacists");

            migrationBuilder.DropColumn(
                name: "ProfileImageUrl",
                table: "pharmacists");

            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "pharmacists");

            migrationBuilder.DropColumn(
                name: "RefreshTokenExpiryTime",
                table: "pharmacists");

            migrationBuilder.DropColumn(
                name: "SecurityStamp",
                table: "pharmacists");

            migrationBuilder.DropColumn(
                name: "TwoFactorEnabled",
                table: "pharmacists");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "pharmacists");

            migrationBuilder.DropColumn(
                name: "VerificationURLExpiresAt",
                table: "pharmacists");

            migrationBuilder.DropColumn(
                name: "AccountType",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FavoritesCount",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "OrdersCount",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "RatesCount",
                table: "AspNetUsers");

            migrationBuilder.RenameTable(
                name: "pharmacists",
                newName: "Pharmacists");

            migrationBuilder.RenameIndex(
                name: "IX_pharmacists_StoreId",
                table: "Pharmacists",
                newName: "IX_Pharmacists_StoreId");

            migrationBuilder.AlterColumn<string>(
                name: "LicenseNumber",
                table: "Pharmacists",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Pharmacists",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Pharmacists",
                table: "Pharmacists",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Admins",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Admins_AspNetUsers_Id",
                        column: x => x.Id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AccountType = table.Column<int>(type: "int", nullable: false),
                    RatesCount = table.Column<int>(type: "int", nullable: false),
                    OrdersCount = table.Column<int>(type: "int", nullable: false),
                    FavoritesCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clients_AspNetUsers_Id",
                        column: x => x.Id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Cart_Clients_ClientId",
                table: "Cart",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Favorite_Clients_ClientId",
                table: "Favorite",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Clients_ClientId",
                table: "Order",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Pharmacists_AspNetUsers_Id",
                table: "Pharmacists",
                column: "Id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Pharmacists_Store_StoreId",
                table: "Pharmacists",
                column: "StoreId",
                principalTable: "Store",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rate_Clients_ClientId",
                table: "Rate",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAddress_Clients_ClientId",
                table: "UserAddress",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cart_Clients_ClientId",
                table: "Cart");

            migrationBuilder.DropForeignKey(
                name: "FK_Favorite_Clients_ClientId",
                table: "Favorite");

            migrationBuilder.DropForeignKey(
                name: "FK_Order_Clients_ClientId",
                table: "Order");

            migrationBuilder.DropForeignKey(
                name: "FK_Pharmacists_AspNetUsers_Id",
                table: "Pharmacists");

            migrationBuilder.DropForeignKey(
                name: "FK_Pharmacists_Store_StoreId",
                table: "Pharmacists");

            migrationBuilder.DropForeignKey(
                name: "FK_Rate_Clients_ClientId",
                table: "Rate");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAddress_Clients_ClientId",
                table: "UserAddress");

            migrationBuilder.DropTable(
                name: "Admins");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Pharmacists",
                table: "Pharmacists");

            migrationBuilder.RenameTable(
                name: "Pharmacists",
                newName: "pharmacists");

            migrationBuilder.RenameIndex(
                name: "IX_Pharmacists_StoreId",
                table: "pharmacists",
                newName: "IX_pharmacists_StoreId");

            migrationBuilder.AlterColumn<string>(
                name: "LicenseNumber",
                table: "pharmacists",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "pharmacists",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<int>(
                name: "AccessFailedCount",
                table: "pharmacists",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateOnly>(
                name: "BirthDate",
                table: "pharmacists",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "pharmacists",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "pharmacists",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailConfirmationLink",
                table: "pharmacists",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmailConfirmed",
                table: "pharmacists",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "pharmacists",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "pharmacists",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "pharmacists",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "LockoutEnabled",
                table: "pharmacists",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LockoutEnd",
                table: "pharmacists",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedEmail",
                table: "pharmacists",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedUserName",
                table: "pharmacists",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OTP",
                table: "pharmacists",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "pharmacists",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "pharmacists",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PhoneNumberConfirmed",
                table: "pharmacists",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ProfileImageUrl",
                table: "pharmacists",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "pharmacists",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefreshTokenExpiryTime",
                table: "pharmacists",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecurityStamp",
                table: "pharmacists",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TwoFactorEnabled",
                table: "pharmacists",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "pharmacists",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerificationURLExpiresAt",
                table: "pharmacists",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "AccountType",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FavoritesCount",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrdersCount",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RatesCount",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_pharmacists",
                table: "pharmacists",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_pharmacists_LicenseNumber",
                table: "pharmacists",
                column: "LicenseNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pharmacists_OTP",
                table: "pharmacists",
                column: "OTP",
                unique: true,
                filter: "[OTP] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Cart_AspNetUsers_ClientId",
                table: "Cart",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Favorite_AspNetUsers_ClientId",
                table: "Favorite",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Order_AspNetUsers_ClientId",
                table: "Order",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_pharmacists_Store_StoreId",
                table: "pharmacists",
                column: "StoreId",
                principalTable: "Store",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Rate_AspNetUsers_ClientId",
                table: "Rate",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAddress_AspNetUsers_ClientId",
                table: "UserAddress",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

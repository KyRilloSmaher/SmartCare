using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCare.InfraStructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAuditTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClassName",
                table: "AuditLogs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "LineNumber",
                table: "AuditLogs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MethodName",
                table: "AuditLogs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Namespace",
                table: "AuditLogs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SourceFile",
                table: "AuditLogs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StackTrace",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ClassName",
                table: "AuditLogs",
                column: "ClassName");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_MethodName",
                table: "AuditLogs",
                column: "MethodName");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Namespace",
                table: "AuditLogs",
                column: "Namespace");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_ClassName",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_MethodName",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_Namespace",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "ClassName",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "LineNumber",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "MethodName",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "Namespace",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "SourceFile",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "StackTrace",
                table: "AuditLogs");
        }
    }
}

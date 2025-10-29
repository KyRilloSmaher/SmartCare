using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCare.InfraStructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRate : Migration
    {
        /// <inheritdoc /> 
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn( name: "TempFlag", table: "Rate");
        }
        /// <inheritdoc /> 
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>
            (
                name: "TempFlag", 
                table: "Rate",
                type: "bit", 
                nullable: false,
                defaultValue: false
             );
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCare.InfraStructure.Migrations
{
    /// <inheritdoc />
    public partial class Updatecarttable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ======================================================
            // 🛒 CART TABLE FIX
            // ======================================================

            // Drop index if it exists
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT name FROM sys.indexes WHERE name = 'IX_Cart_IsActive')
                    DROP INDEX [IX_Cart_IsActive] ON [Cart];
            ");

            // Drop default constraint (important!)
            migrationBuilder.Sql(@"
                DECLARE @dfName NVARCHAR(128);
                SELECT @dfName = d.name
                FROM sys.default_constraints d
                INNER JOIN sys.columns c 
                    ON d.parent_object_id = c.object_id AND d.parent_column_id = c.column_id
                WHERE OBJECT_NAME(d.parent_object_id) = 'Cart' AND c.name = 'IsActive';
                IF @dfName IS NOT NULL EXEC('ALTER TABLE [Cart] DROP CONSTRAINT [' + @dfName + ']');
            ");

            // Drop the IsActive column
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.columns 
                    WHERE Name = N'IsActive' AND Object_ID = Object_ID(N'[Cart]')
                )
                ALTER TABLE [Cart] DROP COLUMN [IsActive];
            ");

            // Add new status column
            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "Cart",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Create index for new column
            migrationBuilder.CreateIndex(
                name: "IX_Cart_status",
                table: "Cart",
                column: "status");


            // ======================================================
            // 🕒 RESERVATION TABLE FIX
            // ======================================================

            // Add new datetime columns
            migrationBuilder.AddColumn<DateTime>(
                name: "ReservedAt_New",
                table: "Reservation",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiredAt_New",
                table: "Reservation",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "DATEADD(DAY, 1, GETDATE())");

            // Convert int UNIX timestamps to datetime
            migrationBuilder.Sql(@"
                UPDATE [Reservation]
                SET 
                    [ReservedAt_New] = 
                        CASE 
                            WHEN ISNUMERIC([ReservedAt]) = 1 THEN DATEADD(SECOND, [ReservedAt], '1970-01-01')
                            ELSE GETDATE()
                        END,
                    [ExpiredAt_New] = 
                        CASE 
                            WHEN ISNUMERIC([ExpiredAt]) = 1 THEN DATEADD(SECOND, [ExpiredAt], '1970-01-01')
                            ELSE DATEADD(DAY, 1, GETDATE())
                        END;
            ");

            // Drop indexes before dropping old columns
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT name FROM sys.indexes WHERE name = 'IX_Reservation_ReservedAt')
                    DROP INDEX [IX_Reservation_ReservedAt] ON [Reservation];
                IF EXISTS (SELECT name FROM sys.indexes WHERE name = 'IX_Reservation_ExpiredAt')
                    DROP INDEX [IX_Reservation_ExpiredAt] ON [Reservation];
            ");

            // Drop default constraints for both columns
            migrationBuilder.Sql(@"
                DECLARE @df1 NVARCHAR(128), @df2 NVARCHAR(128);
                SELECT @df1 = d.name
                FROM sys.default_constraints d
                INNER JOIN sys.columns c ON d.parent_object_id = c.object_id AND d.parent_column_id = c.column_id
                WHERE OBJECT_NAME(d.parent_object_id) = 'Reservation' AND c.name = 'ReservedAt';
                IF @df1 IS NOT NULL EXEC('ALTER TABLE [Reservation] DROP CONSTRAINT [' + @df1 + ']');

                SELECT @df2 = d.name
                FROM sys.default_constraints d
                INNER JOIN sys.columns c ON d.parent_object_id = c.object_id AND d.parent_column_id = c.column_id
                WHERE OBJECT_NAME(d.parent_object_id) = 'Reservation' AND c.name = 'ExpiredAt';
                IF @df2 IS NOT NULL EXEC('ALTER TABLE [Reservation] DROP CONSTRAINT [' + @df2 + ']');
            ");

            // Drop old int columns
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'ReservedAt' AND Object_ID = Object_ID(N'[Reservation]'))
                    ALTER TABLE [Reservation] DROP COLUMN [ReservedAt];
                IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'ExpiredAt' AND Object_ID = Object_ID(N'[Reservation]'))
                    ALTER TABLE [Reservation] DROP COLUMN [ExpiredAt];
            ");

            // Rename new datetime columns
            migrationBuilder.RenameColumn(
                name: "ReservedAt_New",
                table: "Reservation",
                newName: "ReservedAt");

            migrationBuilder.RenameColumn(
                name: "ExpiredAt_New",
                table: "Reservation",
                newName: "ExpiredAt");

            // Recreate indexes
            migrationBuilder.CreateIndex(
                name: "IX_Reservation_ReservedAt",
                table: "Reservation",
                column: "ReservedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_ExpiredAt",
                table: "Reservation",
                column: "ExpiredAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback indexes
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT name FROM sys.indexes WHERE name = 'IX_Reservation_ReservedAt')
                    DROP INDEX [IX_Reservation_ReservedAt] ON [Reservation];
                IF EXISTS (SELECT name FROM sys.indexes WHERE name = 'IX_Reservation_ExpiredAt')
                    DROP INDEX [IX_Reservation_ExpiredAt] ON [Reservation];
            ");

            // Add back int columns
            migrationBuilder.AddColumn<int>(
                name: "ReservedAt_Old",
                table: "Reservation",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ExpiredAt_Old",
                table: "Reservation",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Convert datetime → int
            migrationBuilder.Sql(@"
                UPDATE [Reservation]
                SET 
                    [ReservedAt_Old] = DATEDIFF(SECOND, '1970-01-01', [ReservedAt]),
                    [ExpiredAt_Old] = DATEDIFF(SECOND, '1970-01-01', [ExpiredAt]);
            ");

            // Drop datetime columns
            migrationBuilder.Sql(@"
                ALTER TABLE [Reservation] DROP COLUMN [ReservedAt];
                ALTER TABLE [Reservation] DROP COLUMN [ExpiredAt];
            ");

            // Rename back
            migrationBuilder.RenameColumn(
                name: "ReservedAt_Old",
                table: "Reservation",
                newName: "ReservedAt");

            migrationBuilder.RenameColumn(
                name: "ExpiredAt_Old",
                table: "Reservation",
                newName: "ExpiredAt");

            // --- Cart rollback ---
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT name FROM sys.indexes WHERE name = 'IX_Cart_status')
                    DROP INDEX [IX_Cart_status] ON [Cart];
            ");

            migrationBuilder.Sql(@"
                DECLARE @dfName NVARCHAR(128);
                SELECT @dfName = d.name
                FROM sys.default_constraints d
                INNER JOIN sys.columns c 
                    ON d.parent_object_id = c.object_id AND d.parent_column_id = c.column_id
                WHERE OBJECT_NAME(d.parent_object_id) = 'Cart' AND c.name = 'status';
                IF @dfName IS NOT NULL EXEC('ALTER TABLE [Cart] DROP CONSTRAINT [' + @dfName + ']');
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE [Cart] DROP COLUMN [status];
            ");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Cart",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Cart_IsActive",
                table: "Cart",
                column: "IsActive");
        }
    }
}

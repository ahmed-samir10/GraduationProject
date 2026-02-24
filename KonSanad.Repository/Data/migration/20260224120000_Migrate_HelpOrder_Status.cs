using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KonSanad.Repository.Data.migration
{
    public partial class Migrate_HelpOrder_Status : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add new enum-backed column
            migrationBuilder.AddColumn<int>(
                name: "HelpOrderStatus",
                table: "HelpOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Map existing textual Status values to enum ints
            migrationBuilder.Sql(@"
                UPDATE HelpOrders
                SET HelpOrderStatus = CASE 
                    WHEN LTRIM(RTRIM([Status])) IN (N'قيد الانتظار', N'قيد الانتظار ', 'Pending', 'Requested') THEN 0
                    WHEN LTRIM(RTRIM([Status])) IN (N'مرفوض', 'Rejected') THEN 1
                    WHEN LTRIM(RTRIM([Status])) IN (N'موافق عليه', 'Approved') THEN 2
                    ELSE 0 END
            ");

            // Drop old textual Status column if exists
            // Some databases may not have the column; guard with try/catch via SQL
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'HelpOrders' AND COLUMN_NAME = 'Status')
                BEGIN
                    ALTER TABLE HelpOrders DROP COLUMN [Status]
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Recreate textual Status column
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "HelpOrders",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            // Map enum ints back to localized strings
            migrationBuilder.Sql(@"
                UPDATE HelpOrders
                SET [Status] = CASE HelpOrderStatus
                    WHEN 0 THEN N'قيد الانتظار'
                    WHEN 1 THEN N'مرفوض'
                    WHEN 2 THEN N'موافق عليه'
                    ELSE N'قيد الانتظار' END
            ");

            // Drop the enum column
            migrationBuilder.DropColumn(
                name: "HelpOrderStatus",
                table: "HelpOrders");
        }
    }
}

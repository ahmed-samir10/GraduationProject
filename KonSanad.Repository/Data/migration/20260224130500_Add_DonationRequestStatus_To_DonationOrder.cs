using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KonSanad.Repository.Data.migration
{
    public partial class Add_DonationRequestStatus_To_DonationOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DonationRequestStatus",
                table: "DonationOrders",
                type: "int",
                nullable: true,
                defaultValue: 0);

            // Map existing textual Status values to enum ints
            migrationBuilder.Sql(@"
                UPDATE DonationOrders
                SET DonationRequestStatus = CASE 
                    WHEN LTRIM(RTRIM([Status])) IN (N'قيد الانتظار', N'قيد الانتظار ', 'Pending', 'Requested', N'قيد المراجعة') THEN 0
                    WHEN LTRIM(RTRIM([Status])) IN (N'مكتملة', 'Completed') THEN 1
                    WHEN LTRIM(RTRIM([Status])) IN (N'مرفوض', 'Rejected') THEN 2
                    ELSE 0 END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DonationRequestStatus",
                table: "DonationOrders");
        }
    }
}

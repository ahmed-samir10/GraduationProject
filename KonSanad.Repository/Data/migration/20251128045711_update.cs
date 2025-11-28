using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KonSanad.Repository.Data.migration
{
    /// <inheritdoc />
    public partial class update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DonorType",
                table: "Donor");

            migrationBuilder.AlterColumn<string>(
                name: "NationalId",
                table: "Volunteers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "AppUserId",
                table: "Volunteers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "NationalId",
                table: "Donor",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "AppUserId",
                table: "Donor",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "NationalId",
                table: "Beneficiaries",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "AppUserId",
                table: "Beneficiaries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AppUserId",
                table: "Volunteers");

            migrationBuilder.DropColumn(
                name: "AppUserId",
                table: "Donor");

            migrationBuilder.DropColumn(
                name: "AppUserId",
                table: "Beneficiaries");

            migrationBuilder.AlterColumn<int>(
                name: "NationalId",
                table: "Volunteers",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "NationalId",
                table: "Donor",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "DonorType",
                table: "Donor",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "NationalId",
                table: "Beneficiaries",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}

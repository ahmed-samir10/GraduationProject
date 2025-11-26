using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KonSanad.Repository.Data.migration
{
    /// <inheritdoc />
    public partial class mig4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NationalId",
                table: "Volunteers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NationalId",
                table: "Donor",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NationalId",
                table: "Beneficiaries",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NationalId",
                table: "Volunteers");

            migrationBuilder.DropColumn(
                name: "NationalId",
                table: "Donor");

            migrationBuilder.DropColumn(
                name: "NationalId",
                table: "Beneficiaries");
        }
    }
}

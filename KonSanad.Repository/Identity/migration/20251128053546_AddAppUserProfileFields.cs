using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KonSanad.Repository.Identity.migration
{
    /// <inheritdoc />
    public partial class AddAppUserProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "NationalId",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "NationalId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "AspNetUsers");
        }
    }
}

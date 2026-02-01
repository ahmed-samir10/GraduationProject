using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KonSanad.Repository.Data.migration
{
    /// <inheritdoc />
    public partial class status : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Beneficiaries_AppUser_AppUserId",
                table: "Beneficiaries");

            migrationBuilder.AddColumn<int>(
                name: "MissionStatus",
                table: "Missions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "AppUserId",
                table: "Beneficiaries",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Beneficiaries_AppUser_AppUserId",
                table: "Beneficiaries",
                column: "AppUserId",
                principalTable: "AppUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Beneficiaries_AppUser_AppUserId",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "MissionStatus",
                table: "Missions");

            migrationBuilder.AlterColumn<string>(
                name: "AppUserId",
                table: "Beneficiaries",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_Beneficiaries_AppUser_AppUserId",
                table: "Beneficiaries",
                column: "AppUserId",
                principalTable: "AppUser",
                principalColumn: "Id");
        }
    }
}

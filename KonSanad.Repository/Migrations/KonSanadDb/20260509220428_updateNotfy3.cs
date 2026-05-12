using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KonSanad.Repository.Migrations.KonSanadDb
{
    /// <inheritdoc />
    public partial class updateNotfy3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeadlinePassedNotificationSent",
                table: "Missions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsNearDeadlineNotificationSent",
                table: "Missions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeadlinePassedNotificationSent",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "IsNearDeadlineNotificationSent",
                table: "Missions");
        }
    }
}

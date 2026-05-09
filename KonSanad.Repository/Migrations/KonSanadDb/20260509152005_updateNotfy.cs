using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KonSanad.Repository.Migrations.KonSanadDb
{
    /// <inheritdoc />
    public partial class updateNotfy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsBeneficiariesCompletedNotificationSent",
                table: "Campaigns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsEndDateNotificationSent",
                table: "Campaigns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsGoalReachedNotificationSent",
                table: "Campaigns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsNearEndDateNotificationSent",
                table: "Campaigns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVolunteersCompletedNotificationSent",
                table: "Campaigns",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBeneficiariesCompletedNotificationSent",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "IsEndDateNotificationSent",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "IsGoalReachedNotificationSent",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "IsNearEndDateNotificationSent",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "IsVolunteersCompletedNotificationSent",
                table: "Campaigns");
        }
    }
}

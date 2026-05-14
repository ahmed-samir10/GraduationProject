using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KonSanad.Repository.Migrations.KonSanadDb
{
    /// <inheritdoc />
    public partial class EditCampaignResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReportDate",
                table: "CampaignResult");

            migrationBuilder.DropColumn(
                name: "TotalItemsCount",
                table: "CampaignResult");

            migrationBuilder.AddColumn<int>(
                name: "BeneficiaryCount",
                table: "CampaignResult",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DonorCount",
                table: "CampaignResult",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VolunteerCount",
                table: "CampaignResult",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BeneficiaryCount",
                table: "CampaignResult");

            migrationBuilder.DropColumn(
                name: "DonorCount",
                table: "CampaignResult");

            migrationBuilder.DropColumn(
                name: "VolunteerCount",
                table: "CampaignResult");

            migrationBuilder.AddColumn<DateTime>(
                name: "ReportDate",
                table: "CampaignResult",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalItemsCount",
                table: "CampaignResult",
                type: "int",
                nullable: true);
        }
    }
}

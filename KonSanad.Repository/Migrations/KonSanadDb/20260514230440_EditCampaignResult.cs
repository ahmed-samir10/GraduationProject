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

           

           
        }
    }
}

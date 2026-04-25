using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KonSanad.Repository.Migrations.KonSanadDb
{
    /// <inheritdoc />
    public partial class ben : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_volunteerReports_Beneficiaries_BeneficiaryId",
                table: "volunteerReports");

            migrationBuilder.DropIndex(
                name: "IX_volunteerReports_BeneficiaryId",
                table: "volunteerReports");

            migrationBuilder.AddColumn<int>(
                name: "BeneficiaryId1",
                table: "volunteerReports",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_volunteerReports_BeneficiaryId1",
                table: "volunteerReports",
                column: "BeneficiaryId1");

            migrationBuilder.AddForeignKey(
                name: "FK_volunteerReports_Beneficiaries_BeneficiaryId1",
                table: "volunteerReports",
                column: "BeneficiaryId1",
                principalTable: "Beneficiaries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KonSanad.Repository.Migrations.KonSanadDb
{
    /// <inheritdoc />
    public partial class add : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_volunteerReports_Beneficiaries_BeneficiaryId",
                table: "volunteerReports");

            migrationBuilder.DropIndex(
                name: "IX_volunteerReports_BeneficiaryId",
                table: "volunteerReports");

            migrationBuilder.RenameColumn(
                name: "BenficieryID",
                table: "volunteerReports",
                newName: "BeneficiaryId1");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_volunteerReports_Beneficiaries_BeneficiaryId1",
                table: "volunteerReports");

            migrationBuilder.DropIndex(
                name: "IX_volunteerReports_BeneficiaryId1",
                table: "volunteerReports");

            migrationBuilder.RenameColumn(
                name: "BeneficiaryId1",
                table: "volunteerReports",
                newName: "BenficieryID");

            migrationBuilder.CreateIndex(
                name: "IX_volunteerReports_BeneficiaryId",
                table: "volunteerReports",
                column: "BeneficiaryId");

            migrationBuilder.AddForeignKey(
                name: "FK_volunteerReports_Beneficiaries_BeneficiaryId",
                table: "volunteerReports",
                column: "BeneficiaryId",
                principalTable: "Beneficiaries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

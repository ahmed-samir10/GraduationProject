using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KonSanad.Repository.Migrations.KonSanadDb
{
    /// <inheritdoc />
    public partial class CashDonationStatusANDPagenationonlowstock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HelpOrderid",
                table: "DonationOrders",
                newName: "HelpOrderId");

            migrationBuilder.AddColumn<int>(
                name: "cashDonationStatus",
                table: "CashDonation",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "receiptId",
                table: "CashDonation",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_CashDonation_CampaignId",
                table: "CashDonation",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_CashDonation_receiptId",
                table: "CashDonation",
                column: "receiptId");

            migrationBuilder.AddForeignKey(
                name: "FK_CashDonation_Campaigns_CampaignId",
                table: "CashDonation",
                column: "CampaignId",
                principalTable: "Campaigns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CashDonation_Receipts_receiptId",
                table: "CashDonation",
                column: "receiptId",
                principalTable: "Receipts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CashDonation_Campaigns_CampaignId",
                table: "CashDonation");

            migrationBuilder.DropForeignKey(
                name: "FK_CashDonation_Receipts_receiptId",
                table: "CashDonation");

            migrationBuilder.DropIndex(
                name: "IX_CashDonation_CampaignId",
                table: "CashDonation");

            migrationBuilder.DropIndex(
                name: "IX_CashDonation_receiptId",
                table: "CashDonation");

            migrationBuilder.DropColumn(
                name: "cashDonationStatus",
                table: "CashDonation");

            migrationBuilder.DropColumn(
                name: "receiptId",
                table: "CashDonation");

            migrationBuilder.RenameColumn(
                name: "HelpOrderId",
                table: "DonationOrders",
                newName: "HelpOrderid");
        }
    }
}

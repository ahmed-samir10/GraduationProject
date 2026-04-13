using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KonSanad.Repository.Migrations.KonSanadDb
{
    /// <inheritdoc />
    public partial class miggg223 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CashDonation_Receipts_receiptId",
                table: "CashDonation");

          

            migrationBuilder.AlterColumn<int>(
                name: "receiptId",
                table: "CashDonation",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

          

            migrationBuilder.AddForeignKey(
                name: "FK_CashDonation_Receipts_receiptId",
                table: "CashDonation",
                column: "receiptId",
                principalTable: "Receipts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CashDonation_Receipts_receiptId",
                table: "CashDonation");

            migrationBuilder.DropColumn(
                name: "ReajectReason",
                table: "Volunteers");

            migrationBuilder.DropColumn(
                name: "RejectReason",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ReajectReason",
                table: "CampaignVolunteers");

            migrationBuilder.AlterColumn<int>(
                name: "receiptId",
                table: "CashDonation",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CashDonation_Receipts_receiptId",
                table: "CashDonation",
                column: "receiptId",
                principalTable: "Receipts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

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
            // ✅ RenameColumn safely
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                           WHERE TABLE_NAME = 'DonationOrders' AND COLUMN_NAME = 'HelpOrderid')
                BEGIN EXEC sp_rename N'[DonationOrders].[HelpOrderid]', N'HelpOrderId', 'COLUMN' END
            ");

            // ✅ AddColumn safely
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                               WHERE TABLE_NAME = 'CashDonation' AND COLUMN_NAME = 'cashDonationStatus')
                BEGIN ALTER TABLE [CashDonation] ADD [cashDonationStatus] int NOT NULL DEFAULT 0 END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                               WHERE TABLE_NAME = 'CashDonation' AND COLUMN_NAME = 'receiptId')
                BEGIN ALTER TABLE [CashDonation] ADD [receiptId] int NOT NULL DEFAULT 0 END
            ");

            // ✅ CreateIndex safely
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.indexes 
                               WHERE name = 'IX_CashDonation_CampaignId' AND object_id = OBJECT_ID('CashDonation'))
                BEGIN CREATE INDEX [IX_CashDonation_CampaignId] ON [CashDonation] ([CampaignId]) END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.indexes 
                               WHERE name = 'IX_CashDonation_receiptId' AND object_id = OBJECT_ID('CashDonation'))
                BEGIN CREATE INDEX [IX_CashDonation_receiptId] ON [CashDonation] ([receiptId]) END
            ");

            // ✅ AddForeignKey safely
            migrationBuilder.Sql(@"
    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
                   WHERE CONSTRAINT_NAME = 'FK_CashDonation_Campaigns_CampaignId')
    BEGIN
        ALTER TABLE [CashDonation] WITH NOCHECK 
        ADD CONSTRAINT [FK_CashDonation_Campaigns_CampaignId]
        FOREIGN KEY ([CampaignId]) REFERENCES [Campaigns] ([Id])
    END
");

            migrationBuilder.Sql(@"
    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
                   WHERE CONSTRAINT_NAME = 'FK_CashDonation_Receipts_receiptId')
    BEGIN
        ALTER TABLE [CashDonation] WITH NOCHECK 
        ADD CONSTRAINT [FK_CashDonation_Receipts_receiptId]
        FOREIGN KEY ([receiptId]) REFERENCES [Receipts] ([Id]) ON DELETE CASCADE
    END
");
        }

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

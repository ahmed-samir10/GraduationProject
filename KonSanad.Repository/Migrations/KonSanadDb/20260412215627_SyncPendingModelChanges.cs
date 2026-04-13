using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KonSanad.Repository.Migrations.KonSanadDb
{
    /// <inheritdoc />
    public partial class SyncPendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF COL_LENGTH('Volunteers', 'ReajectReason') IS NULL
                BEGIN
                    ALTER TABLE [Volunteers] ADD [ReajectReason] nvarchar(max) NULL;
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('OrderItems', 'RejectReason') IS NULL
                BEGIN
                    ALTER TABLE [OrderItems] ADD [RejectReason] nvarchar(max) NULL;
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('CampaignVolunteers', 'ReajectReason') IS NULL
                BEGIN
                    ALTER TABLE [CampaignVolunteers] ADD [ReajectReason] nvarchar(max) NULL;
                END
                """);

            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT 1
                    FROM sys.foreign_keys
                    WHERE name = 'FK_CashDonation_Receipts_receiptId'
                      AND parent_object_id = OBJECT_ID(N'[CashDonation]')
                )
                BEGIN
                    ALTER TABLE [CashDonation] DROP CONSTRAINT [FK_CashDonation_Receipts_receiptId];
                END
                """);

            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT 1
                    FROM sys.columns c
                    INNER JOIN sys.tables t ON c.object_id = t.object_id
                    WHERE t.name = 'CashDonation'
                      AND c.name = 'receiptId'
                      AND c.is_nullable = 0
                )
                BEGIN
                    DECLARE @defaultConstraint sysname;

                    SELECT @defaultConstraint = dc.name
                    FROM sys.default_constraints dc
                    INNER JOIN sys.columns c
                        ON dc.parent_object_id = c.object_id
                       AND dc.parent_column_id = c.column_id
                    INNER JOIN sys.tables t
                        ON t.object_id = c.object_id
                    WHERE t.name = 'CashDonation'
                      AND c.name = 'receiptId';

                    IF @defaultConstraint IS NOT NULL
                    BEGIN
                        EXEC(N'ALTER TABLE [CashDonation] DROP CONSTRAINT [' + @defaultConstraint + ']');
                    END

                    ALTER TABLE [CashDonation] ALTER COLUMN [receiptId] int NULL;
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('CashDonation', 'receiptId') IS NOT NULL
                BEGIN
                    UPDATE cd
                    SET cd.[receiptId] = NULL
                    FROM [CashDonation] cd
                    LEFT JOIN [Receipts] r ON r.[Id] = cd.[receiptId]
                    WHERE cd.[receiptId] IS NOT NULL
                      AND r.[Id] IS NULL;
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('CashDonation', 'receiptId') IS NOT NULL
                   AND NOT EXISTS (
                       SELECT 1
                       FROM sys.foreign_keys
                       WHERE name = 'FK_CashDonation_Receipts_receiptId'
                         AND parent_object_id = OBJECT_ID(N'[CashDonation]')
                   )
                BEGIN
                    ALTER TABLE [CashDonation]
                    ADD CONSTRAINT [FK_CashDonation_Receipts_receiptId]
                    FOREIGN KEY ([receiptId]) REFERENCES [Receipts]([Id]);
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT 1
                    FROM sys.foreign_keys
                    WHERE name = 'FK_CashDonation_Receipts_receiptId'
                      AND parent_object_id = OBJECT_ID(N'[CashDonation]')
                )
                BEGIN
                    ALTER TABLE [CashDonation] DROP CONSTRAINT [FK_CashDonation_Receipts_receiptId];
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('CampaignVolunteers', 'ReajectReason') IS NOT NULL
                BEGIN
                    ALTER TABLE [CampaignVolunteers] DROP COLUMN [ReajectReason];
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('OrderItems', 'RejectReason') IS NOT NULL
                BEGIN
                    ALTER TABLE [OrderItems] DROP COLUMN [RejectReason];
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('Volunteers', 'ReajectReason') IS NOT NULL
                BEGIN
                    ALTER TABLE [Volunteers] DROP COLUMN [ReajectReason];
                END
                """);

            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT 1
                    FROM sys.columns c
                    INNER JOIN sys.tables t ON c.object_id = t.object_id
                    WHERE t.name = 'CashDonation'
                      AND c.name = 'receiptId'
                      AND c.is_nullable = 1
                )
                BEGIN
                    UPDATE [CashDonation]
                    SET [receiptId] = 0
                    WHERE [receiptId] IS NULL;

                    ALTER TABLE [CashDonation] ALTER COLUMN [receiptId] int NOT NULL;
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('CashDonation', 'receiptId') IS NOT NULL
                   AND NOT EXISTS (
                       SELECT 1
                       FROM sys.foreign_keys
                       WHERE name = 'FK_CashDonation_Receipts_receiptId'
                         AND parent_object_id = OBJECT_ID(N'[CashDonation]')
                   )
                BEGIN
                    ALTER TABLE [CashDonation]
                    ADD CONSTRAINT [FK_CashDonation_Receipts_receiptId]
                    FOREIGN KEY ([receiptId]) REFERENCES [Receipts]([Id]) ON DELETE CASCADE;
                END
                """);
        }
    }
}

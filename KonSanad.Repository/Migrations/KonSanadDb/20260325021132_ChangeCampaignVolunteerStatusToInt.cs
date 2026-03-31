using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KonSanad.Repository.Migrations.KonSanadDb
{
    /// <inheritdoc />
    public partial class ChangeCampaignVolunteerStatusToInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Helper: ALTER COLUMN Status string -> int NULL (for each table)
            var tablesWithStatus = new[]
            {
                "Volunteers", "VolunteerMissions", "SuppliesMovement", "Supplies",
                "Receipts", "Missions", "Managers", "HelpOrders", "HelpOrderItem",
                "Donor", "DonationOrders", "Categories", "CashDonation", "Carts",
                "CartItems", "Campaigns", "CampaignResult", "BeneficiaryCampaign", "Beneficiaries"
            };

            foreach (var table in tablesWithStatus)
            {
                migrationBuilder.Sql($@"
                    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{table}')
                    AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{table}' AND COLUMN_NAME = 'Status')
                    BEGIN
                        DECLARE @var_{table} sysname;
                        SELECT @var_{table} = [d].[name]
                        FROM [sys].[default_constraints] [d]
                        INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
                        WHERE ([d].[parent_object_id] = OBJECT_ID(N'[{table}]') AND [c].[name] = N'Status');
                        IF @var_{table} IS NOT NULL EXEC(N'ALTER TABLE [{table}] DROP CONSTRAINT [' + @var_{table} + '];');
                        ALTER TABLE [{table}] ALTER COLUMN [Status] int NULL;
                    END
                ");
            }

            // HelpOrders.HelpOrderStatus default value: 0 -> 2
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                           WHERE TABLE_NAME = 'HelpOrders' AND COLUMN_NAME = 'HelpOrderStatus')
                BEGIN
                    DECLARE @varHOS sysname;
                    SELECT @varHOS = [d].[name]
                    FROM [sys].[default_constraints] [d]
                    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
                    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[HelpOrders]') AND [c].[name] = N'HelpOrderStatus');
                    IF @varHOS IS NOT NULL EXEC(N'ALTER TABLE [HelpOrders] DROP CONSTRAINT [' + @varHOS + '];');
                    ALTER TABLE [HelpOrders] ADD CONSTRAINT [DF_HelpOrders_HelpOrderStatus] DEFAULT 2 FOR [HelpOrderStatus];
                END
            ");

            // CampaignVolunteers.Status string -> int NOT NULL DEFAULT 0
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CampaignVolunteers')
                AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CampaignVolunteers' AND COLUMN_NAME = 'Status')
                BEGIN
                    DECLARE @varCV sysname;
                    SELECT @varCV = [d].[name]
                    FROM [sys].[default_constraints] [d]
                    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
                    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CampaignVolunteers]') AND [c].[name] = N'Status');
                    IF @varCV IS NOT NULL EXEC(N'ALTER TABLE [CampaignVolunteers] DROP CONSTRAINT [' + @varCV + '];');
                    ALTER TABLE [CampaignVolunteers] ALTER COLUMN [Status] int NOT NULL;
                    ALTER TABLE [CampaignVolunteers] ADD CONSTRAINT [DF_CampaignVolunteers_Status] DEFAULT 0 FOR [Status];
                END
            ");

            // DonationOrders.DonationRequestStatus ADD if not exists
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'DonationOrders')
                AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                                WHERE TABLE_NAME = 'DonationOrders' AND COLUMN_NAME = 'DonationRequestStatus')
                BEGIN
                    ALTER TABLE [DonationOrders] ADD [DonationRequestStatus] int NULL;
                END
            ");
        }
        /// <inheritdoc />

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DonationRequestStatus",
                table: "DonationOrders");

            // باقي الـ Down تفضل زي ما هي
            migrationBuilder.AlterColumn<string>(name: "Status", table: "Volunteers", type: "nvarchar(max)", nullable: false, defaultValue: "", oldClrType: typeof(int), oldType: "int", oldNullable: true);
            migrationBuilder.AlterColumn<string>(name: "Status", table: "VolunteerMissions", type: "nvarchar(max)", nullable: false, defaultValue: "", oldClrType: typeof(int), oldType: "int", oldNullable: true);
            migrationBuilder.AlterColumn<string>(name: "Status", table: "SuppliesMovement", type: "nvarchar(max)", nullable: false, defaultValue: "", oldClrType: typeof(int), oldType: "int", oldNullable: true);
            migrationBuilder.AlterColumn<string>(name: "Status", table: "Supplies", type: "nvarchar(max)", nullable: false, defaultValue: "", oldClrType: typeof(int), oldType: "int", oldNullable: true);
            migrationBuilder.AlterColumn<string>(name: "Status", table: "Receipts", type: "nvarchar(max)", nullable: false, defaultValue: "", oldClrType: typeof(int), oldType: "int", oldNullable: true);
            migrationBuilder.AlterColumn<string>(name: "Status", table: "Missions", type: "nvarchar(max)", nullable: false, defaultValue: "", oldClrType: typeof(int), oldType: "int", oldNullable: true);
            migrationBuilder.AlterColumn<string>(name: "Status", table: "Managers", type: "nvarchar(max)", nullable: false, defaultValue: "", oldClrType: typeof(int), oldType: "int", oldNullable: true);
            migrationBuilder.AlterColumn<string>(name: "Status", table: "HelpOrders", type: "nvarchar(max)", nullable: false, defaultValue: "", oldClrType: typeof(int), oldType: "int", oldNullable: true);
            migrationBuilder.AlterColumn<int>(name: "HelpOrderStatus", table: "HelpOrders", type: "int", nullable: false, defaultValue: 0, oldClrType: typeof(int), oldType: "int", oldDefaultValue: 2);
            migrationBuilder.AlterColumn<string>(name: "Status", table: "HelpOrderItem", type: "nvarchar(max)", nullable: false, defaultValue: "", oldClrType: typeof(int), oldType: "int", oldNullable: true);
            migrationBuilder.AlterColumn<string>(name: "Status", table: "Donor", type: "nvarchar(max)", nullable: false, defaultValue: "", oldClrType: typeof(int), oldType: "int", oldNullable: true);
            migrationBuilder.AlterColumn<string>(name: "Status", table: "DonationOrders", type: "nvarchar(max)", nullable: false, defaultValue: "", oldClrType: typeof(int), oldType: "int", oldNullable: true);
            migrationBuilder.AlterColumn<string>(name: "Status", table: "Categories", type: "nvarchar(max)", nullable: false, defaultValue: "", oldClrType: typeof(int), oldType: "int", oldNullable: true);
            migrationBuilder.AlterColumn<string>(name: "Status", table: "CashDonation", type: "nvarchar(max)", nullable: false, defaultValue: "", oldClrType: typeof(int), oldType: "int", oldNullable: true);
            migrationBuilder.AlterColumn<string>(name: "Status", table: "Carts", type: "nvarchar(max)", nullable: false, defaultValue: "", oldClrType: typeof(int), oldType: "int", oldNullable: true);
            migrationBuilder.AlterColumn<string>(name: "Status", table: "CartItems", type: "nvarchar(max)", nullable: false, defaultValue: "", oldClrType: typeof(int), oldType: "int", oldNullable: true);
            migrationBuilder.AlterColumn<string>(name: "Status", table: "CampaignVolunteers", type: "nvarchar(50)", maxLength: 50, nullable: true, oldClrType: typeof(int), oldType: "int", oldMaxLength: 50);
            migrationBuilder.AlterColumn<string>(name: "Status", table: "Campaigns", type: "nvarchar(max)", nullable: false, defaultValue: "", oldClrType: typeof(int), oldType: "int", oldNullable: true);
            migrationBuilder.AlterColumn<string>(name: "Status", table: "CampaignResult", type: "nvarchar(max)", nullable: false, defaultValue: "", oldClrType: typeof(int), oldType: "int", oldNullable: true);
            migrationBuilder.AlterColumn<string>(name: "Status", table: "BeneficiaryCampaign", type: "nvarchar(max)", nullable: false, defaultValue: "", oldClrType: typeof(int), oldType: "int", oldNullable: true);
            migrationBuilder.AlterColumn<string>(name: "Status", table: "Beneficiaries", type: "nvarchar(max)", nullable: false, defaultValue: "", oldClrType: typeof(int), oldType: "int", oldNullable: true);
        }
    }
}

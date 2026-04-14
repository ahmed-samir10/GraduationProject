using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KonSanad.Repository.Data.migration
{
    /// <inheritdoc />
    public partial class GetMyRequestFromOrderItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ✅ Drop columns safely
            migrationBuilder.Sql(@"
        IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                   WHERE TABLE_NAME = 'HelpOrders' AND COLUMN_NAME = 'Description')
        BEGIN ALTER TABLE [HelpOrders] DROP COLUMN [Description] END
    ");

            migrationBuilder.Sql(@"
        IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                   WHERE TABLE_NAME = 'HelpOrders' AND COLUMN_NAME = 'Title')
        BEGIN ALTER TABLE [HelpOrders] DROP COLUMN [Title] END
    ");

            // ✅ Alter column safely
            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "OrderItems",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            // ✅ Add columns safely (IF NOT EXISTS)
            migrationBuilder.Sql(@"
        IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                       WHERE TABLE_NAME = 'OrderItems' AND COLUMN_NAME = 'orderItemStatus')
        BEGIN ALTER TABLE [OrderItems] ADD [orderItemStatus] int NOT NULL DEFAULT 0 END
    ");

            migrationBuilder.Sql(@"
        IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                       WHERE TABLE_NAME = 'HelpOrders' AND COLUMN_NAME = 'HelpOrderStatus')
        BEGIN ALTER TABLE [HelpOrders] ADD [HelpOrderStatus] int NOT NULL DEFAULT 0 END
    ");

            migrationBuilder.Sql(@"
        IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                       WHERE TABLE_NAME = 'HelpOrderItem' AND COLUMN_NAME = 'HelpOrderStatus')
        BEGIN ALTER TABLE [HelpOrderItem] ADD [HelpOrderStatus] int NOT NULL DEFAULT 0 END
    ");

            migrationBuilder.Sql(@"
        IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                       WHERE TABLE_NAME = 'HelpOrderItem' AND COLUMN_NAME = 'Title')
        BEGIN ALTER TABLE [HelpOrderItem] ADD [Title] nvarchar(max) NULL END
    ");

            migrationBuilder.Sql(@"
        IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                       WHERE TABLE_NAME = 'BeneficiaryCampaign' AND COLUMN_NAME = 'Reason')
        BEGIN ALTER TABLE [BeneficiaryCampaign] ADD [Reason] nvarchar(max) NOT NULL DEFAULT '' END
    ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "orderItemStatus",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "HelpOrderStatus",
                table: "HelpOrders");

            migrationBuilder.DropColumn(
                name: "HelpOrderStatus",
                table: "HelpOrderItem");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "HelpOrderItem");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "BeneficiaryCampaign");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "OrderItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "HelpOrders",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "HelpOrders",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}

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
            migrationBuilder.DropColumn(
                name: "Description",
                table: "HelpOrders");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "HelpOrders");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "OrderItems",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "orderItemStatus",
                table: "OrderItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HelpOrderStatus",
                table: "HelpOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HelpOrderStatus",
                table: "HelpOrderItem",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "HelpOrderItem",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "BeneficiaryCampaign",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
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

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KonSanad.Repository.Data.migration
{
    /// <inheritdoc />
    public partial class FirstMigrate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Beneficiaries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Pssword = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Beneficiaries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Donor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DonorType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Donor", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Managers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Managers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Supplies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StockQuantity = table.Column<int>(type: "int", nullable: true),
                    MinimumLevel = table.Column<int>(type: "int", nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Supplies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Supplies_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Campaigns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GoalCash = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    GoalSupplies = table.Column<int>(type: "int", nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: true),
                    ManagerId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campaigns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Campaigns_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Campaigns_Managers_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "Managers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "HelpOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    BeneficiaryId = table.Column<int>(type: "int", nullable: true),
                    ManagerId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HelpOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HelpOrders_Beneficiaries_BeneficiaryId",
                        column: x => x.BeneficiaryId,
                        principalTable: "Beneficiaries",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HelpOrders_Managers_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "Managers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Volunteers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Skills = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Availability = table.Column<bool>(type: "bit", nullable: true),
                    ManagerId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Volunteers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Volunteers_Managers_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "Managers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CampaignResult",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CampaignId = table.Column<int>(type: "int", nullable: false),
                    ReportDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalCashCollected = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotalItemsCount = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignResult", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignResult_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DonationOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DonationCategoryStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CashAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DonorId = table.Column<int>(type: "int", nullable: true),
                    CampaignId = table.Column<int>(type: "int", nullable: true),
                    ManagerId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonationOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DonationOrders_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DonationOrders_Donor_DonorId",
                        column: x => x.DonorId,
                        principalTable: "Donor",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DonationOrders_Managers_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "Managers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "HelpOrderItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuantityRequested = table.Column<int>(type: "int", nullable: false),
                    HelpOrderId = table.Column<int>(type: "int", nullable: false),
                    SupplyId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HelpOrderItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HelpOrderItem_HelpOrders_HelpOrderId",
                        column: x => x.HelpOrderId,
                        principalTable: "HelpOrders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HelpOrderItem_Supplies_SupplyId",
                        column: x => x.SupplyId,
                        principalTable: "Supplies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CampaignVolunteers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CampaignId = table.Column<int>(type: "int", nullable: false),
                    VolunteerId = table.Column<int>(type: "int", nullable: false),
                    JoinedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignVolunteers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignVolunteers_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CampaignVolunteers_Volunteers_VolunteerId",
                        column: x => x.VolunteerId,
                        principalTable: "Volunteers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Missions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ManagerId = table.Column<int>(type: "int", nullable: true),
                    VolunteerId = table.Column<int>(type: "int", nullable: true),
                    CampaignId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Missions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Missions_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Missions_Managers_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "Managers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Missions_Volunteers_VolunteerId",
                        column: x => x.VolunteerId,
                        principalTable: "Volunteers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CashDonation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DonationOrderId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashDonation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashDonation_DonationOrders_DonationOrderId",
                        column: x => x.DonationOrderId,
                        principalTable: "DonationOrders",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomSupplyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DonationOrderId = table.Column<int>(type: "int", nullable: false),
                    SupplyId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_DonationOrders_DonationOrderId",
                        column: x => x.DonationOrderId,
                        principalTable: "DonationOrders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderItems_Supplies_SupplyId",
                        column: x => x.SupplyId,
                        principalTable: "Supplies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Receipts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DonationOrderId = table.Column<int>(type: "int", nullable: true),
                    ManagerId = table.Column<int>(type: "int", nullable: true),
                    BeneficiaryId = table.Column<int>(type: "int", nullable: true),
                    DonorId = table.Column<int>(type: "int", nullable: true),
                    CashDonationId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Receipts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Receipts_Beneficiaries_BeneficiaryId",
                        column: x => x.BeneficiaryId,
                        principalTable: "Beneficiaries",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Receipts_CashDonation_CashDonationId",
                        column: x => x.CashDonationId,
                        principalTable: "CashDonation",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Receipts_DonationOrders_DonationOrderId",
                        column: x => x.DonationOrderId,
                        principalTable: "DonationOrders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Receipts_Donor_DonorId",
                        column: x => x.DonorId,
                        principalTable: "Donor",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Receipts_Managers_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "Managers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SuppliesMovement",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplyId = table.Column<int>(type: "int", nullable: true),
                    OrderItemId = table.Column<int>(type: "int", nullable: true),
                    BeneficiaryID = table.Column<int>(type: "int", nullable: true),
                    ManagerId = table.Column<int>(type: "int", nullable: true),
                    MovementType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MovedQuantity = table.Column<double>(type: "float", nullable: true),
                    MovementDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuppliesMovement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SuppliesMovement_Beneficiaries_BeneficiaryID",
                        column: x => x.BeneficiaryID,
                        principalTable: "Beneficiaries",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SuppliesMovement_Managers_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "Managers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SuppliesMovement_OrderItems_OrderItemId",
                        column: x => x.OrderItemId,
                        principalTable: "OrderItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SuppliesMovement_Supplies_SupplyId",
                        column: x => x.SupplyId,
                        principalTable: "Supplies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignResult_CampaignId",
                table: "CampaignResult",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_CategoryId",
                table: "Campaigns",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_ManagerId",
                table: "Campaigns",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignVolunteer_Campaign_Volunteer",
                table: "CampaignVolunteers",
                columns: new[] { "CampaignId", "VolunteerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CampaignVolunteers_VolunteerId",
                table: "CampaignVolunteers",
                column: "VolunteerId");

            migrationBuilder.CreateIndex(
                name: "IX_CashDonation_DonationOrderId",
                table: "CashDonation",
                column: "DonationOrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DonationOrders_CampaignId",
                table: "DonationOrders",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_DonationOrders_DonorId",
                table: "DonationOrders",
                column: "DonorId");

            migrationBuilder.CreateIndex(
                name: "IX_DonationOrders_ManagerId",
                table: "DonationOrders",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_HelpOrderItem_HelpOrderId",
                table: "HelpOrderItem",
                column: "HelpOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_HelpOrderItem_SupplyId",
                table: "HelpOrderItem",
                column: "SupplyId");

            migrationBuilder.CreateIndex(
                name: "IX_HelpOrders_BeneficiaryId",
                table: "HelpOrders",
                column: "BeneficiaryId");

            migrationBuilder.CreateIndex(
                name: "IX_HelpOrders_ManagerId",
                table: "HelpOrders",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Missions_CampaignId",
                table: "Missions",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_Missions_ManagerId",
                table: "Missions",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Missions_VolunteerId",
                table: "Missions",
                column: "VolunteerId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_DonationOrderId",
                table: "OrderItems",
                column: "DonationOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_SupplyId",
                table: "OrderItems",
                column: "SupplyId");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_BeneficiaryId",
                table: "Receipts",
                column: "BeneficiaryId");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_CashDonationId",
                table: "Receipts",
                column: "CashDonationId");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_DonationOrderId",
                table: "Receipts",
                column: "DonationOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_DonorId",
                table: "Receipts",
                column: "DonorId");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_ManagerId",
                table: "Receipts",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Supplies_CategoryId",
                table: "Supplies",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SuppliesMovement_BeneficiaryID",
                table: "SuppliesMovement",
                column: "BeneficiaryID");

            migrationBuilder.CreateIndex(
                name: "IX_SuppliesMovement_ManagerId",
                table: "SuppliesMovement",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_SuppliesMovement_OrderItemId",
                table: "SuppliesMovement",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SuppliesMovement_SupplyId",
                table: "SuppliesMovement",
                column: "SupplyId");

            migrationBuilder.CreateIndex(
                name: "IX_Volunteers_ManagerId",
                table: "Volunteers",
                column: "ManagerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CampaignResult");

            migrationBuilder.DropTable(
                name: "CampaignVolunteers");

            migrationBuilder.DropTable(
                name: "HelpOrderItem");

            migrationBuilder.DropTable(
                name: "Missions");

            migrationBuilder.DropTable(
                name: "Receipts");

            migrationBuilder.DropTable(
                name: "SuppliesMovement");

            migrationBuilder.DropTable(
                name: "HelpOrders");

            migrationBuilder.DropTable(
                name: "Volunteers");

            migrationBuilder.DropTable(
                name: "CashDonation");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Beneficiaries");

            migrationBuilder.DropTable(
                name: "DonationOrders");

            migrationBuilder.DropTable(
                name: "Supplies");

            migrationBuilder.DropTable(
                name: "Campaigns");

            migrationBuilder.DropTable(
                name: "Donor");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Managers");
        }
    }
}

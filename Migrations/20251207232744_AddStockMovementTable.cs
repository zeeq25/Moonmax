using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moonmax.Migrations
{
    /// <inheritdoc />
    public partial class AddStockMovementTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StockMovements",
                columns: table => new
                {
                    MovementID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventoryID = table.Column<int>(type: "int", nullable: false),
                    MovementType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    PreviousQuantity = table.Column<int>(type: "int", nullable: false),
                    NewQuantity = table.Column<int>(type: "int", nullable: false),
                    PurchaseOrderID = table.Column<int>(type: "int", nullable: true),
                    JobOrderID = table.Column<int>(type: "int", nullable: true),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    MovementDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockMovements", x => x.MovementID);
                    table.ForeignKey(
                        name: "FK_StockMovements_Inventories_InventoryID",
                        column: x => x.InventoryID,
                        principalTable: "Inventories",
                        principalColumn: "InventoryID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StockMovements_JobOrders_JobOrderID",
                        column: x => x.JobOrderID,
                        principalTable: "JobOrders",
                        principalColumn: "JobID");
                    table.ForeignKey(
                        name: "FK_StockMovements_PurchaseOrder_PurchaseOrderID",
                        column: x => x.PurchaseOrderID,
                        principalTable: "PurchaseOrder",
                        principalColumn: "PurchaseOrderID");
                    table.ForeignKey(
                        name: "FK_StockMovements_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_InventoryID",
                table: "StockMovements",
                column: "InventoryID");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_JobOrderID",
                table: "StockMovements",
                column: "JobOrderID");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_PurchaseOrderID",
                table: "StockMovements",
                column: "PurchaseOrderID");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_UserID",
                table: "StockMovements",
                column: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockMovements");
        }
    }
}

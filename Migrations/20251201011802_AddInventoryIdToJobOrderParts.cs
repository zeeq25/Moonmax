using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moonmax.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryIdToJobOrderParts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InventoryID",
                table: "JobOrderParts",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_JobOrderParts_InventoryID",
                table: "JobOrderParts",
                column: "InventoryID");

            migrationBuilder.AddForeignKey(
                name: "FK_JobOrderParts_Inventories_InventoryID",
                table: "JobOrderParts",
                column: "InventoryID",
                principalTable: "Inventories",
                principalColumn: "InventoryID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobOrderParts_Inventories_InventoryID",
                table: "JobOrderParts");

            migrationBuilder.DropIndex(
                name: "IX_JobOrderParts_InventoryID",
                table: "JobOrderParts");

            migrationBuilder.DropColumn(
                name: "InventoryID",
                table: "JobOrderParts");
        }
    }
}

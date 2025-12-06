using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moonmax.Migrations
{
    public partial class CreateSupplierTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Create Suppliers table
            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    SupplierID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ContactNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ProductLine = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PaymentTerms = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.SupplierID);
                });

            // 2. Add SupplierID column to Inventories (nullable for now to avoid FK conflicts)
            migrationBuilder.AddColumn<int>(
                name: "SupplierID",
                table: "Inventories",
                type: "int",
                nullable: true);

            // 3. Create index on SupplierID
            migrationBuilder.CreateIndex(
                name: "IX_Inventories_SupplierID",
                table: "Inventories",
                column: "SupplierID");

            // 4. Add foreign key constraint
            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_Suppliers_SupplierID",
                table: "Inventories",
                column: "SupplierID",
                principalTable: "Suppliers",
                principalColumn: "SupplierID",
                onDelete: ReferentialAction.Restrict); // Restrict so existing data won’t break
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_Suppliers_SupplierID",
                table: "Inventories");

            migrationBuilder.DropIndex(
                name: "IX_Inventories_SupplierID",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "SupplierID",
                table: "Inventories");

            migrationBuilder.DropTable(
                name: "Suppliers");
        }
    }
}

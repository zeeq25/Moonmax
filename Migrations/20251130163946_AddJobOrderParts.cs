using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moonmax.Migrations
{
    /// <inheritdoc />
    public partial class AddJobOrderParts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "WalkInContactNumber",
                table: "JobOrders",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "JobOrderParts",
                columns: table => new
                {
                    JobOrderPartID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobID = table.Column<int>(type: "int", nullable: false),
                    JobOrderJobID = table.Column<int>(type: "int", nullable: true),
                    PartName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobOrderParts", x => x.JobOrderPartID);
                    table.ForeignKey(
                        name: "FK_JobOrderParts_JobOrders_JobOrderJobID",
                        column: x => x.JobOrderJobID,
                        principalTable: "JobOrders",
                        principalColumn: "JobID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobOrderParts_JobOrderJobID",
                table: "JobOrderParts",
                column: "JobOrderJobID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobOrderParts");

            migrationBuilder.AlterColumn<string>(
                name: "WalkInContactNumber",
                table: "JobOrders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}

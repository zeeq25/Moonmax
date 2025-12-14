using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moonmax.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobOrderParts_JobOrders_JobOrderJobID",
                table: "JobOrderParts");

            migrationBuilder.DropForeignKey(
                name: "FK_JobOrders_Client_ClientID",
                table: "JobOrders");

            migrationBuilder.AlterColumn<int>(
                name: "ClientID",
                table: "JobOrders",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "JobOrderJobID",
                table: "JobOrderParts",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    AuditID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TargetID = table.Column<int>(type: "int", nullable: true),
                    Module = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.AuditID);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_JobOrderParts_JobOrders_JobOrderJobID",
                table: "JobOrderParts",
                column: "JobOrderJobID",
                principalTable: "JobOrders",
                principalColumn: "JobID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_JobOrders_Client_ClientID",
                table: "JobOrders",
                column: "ClientID",
                principalTable: "Client",
                principalColumn: "ClientID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobOrderParts_JobOrders_JobOrderJobID",
                table: "JobOrderParts");

            migrationBuilder.DropForeignKey(
                name: "FK_JobOrders_Client_ClientID",
                table: "JobOrders");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.AlterColumn<int>(
                name: "ClientID",
                table: "JobOrders",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "JobOrderJobID",
                table: "JobOrderParts",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_JobOrderParts_JobOrders_JobOrderJobID",
                table: "JobOrderParts",
                column: "JobOrderJobID",
                principalTable: "JobOrders",
                principalColumn: "JobID");

            migrationBuilder.AddForeignKey(
                name: "FK_JobOrders_Client_ClientID",
                table: "JobOrders",
                column: "ClientID",
                principalTable: "Client",
                principalColumn: "ClientID");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moonmax.Migrations
{
    /// <inheritdoc />
    public partial class AddWalkInContactToJobOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobOrders_Client_ClientId",
                table: "JobOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_JobOrders_Technician_TechnicianId",
                table: "JobOrders");

            migrationBuilder.RenameColumn(
                name: "TechnicianId",
                table: "Technician",
                newName: "TechnicianID");

            migrationBuilder.RenameColumn(
                name: "TechnicianId",
                table: "JobOrders",
                newName: "TechnicianID");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "JobOrders",
                newName: "ClientID");

            migrationBuilder.RenameIndex(
                name: "IX_JobOrders_TechnicianId",
                table: "JobOrders",
                newName: "IX_JobOrders_TechnicianID");

            migrationBuilder.RenameIndex(
                name: "IX_JobOrders_ClientId",
                table: "JobOrders",
                newName: "IX_JobOrders_ClientID");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "Client",
                newName: "ClientID");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DueDate",
                table: "JobOrders",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<int>(
                name: "ClientID",
                table: "JobOrders",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "WalkInContactNumber",
                table: "JobOrders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_JobOrders_Client_ClientID",
                table: "JobOrders",
                column: "ClientID",
                principalTable: "Client",
                principalColumn: "ClientID");

            migrationBuilder.AddForeignKey(
                name: "FK_JobOrders_Technician_TechnicianID",
                table: "JobOrders",
                column: "TechnicianID",
                principalTable: "Technician",
                principalColumn: "TechnicianID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobOrders_Client_ClientID",
                table: "JobOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_JobOrders_Technician_TechnicianID",
                table: "JobOrders");

            migrationBuilder.DropColumn(
                name: "WalkInContactNumber",
                table: "JobOrders");

            migrationBuilder.RenameColumn(
                name: "TechnicianID",
                table: "Technician",
                newName: "TechnicianId");

            migrationBuilder.RenameColumn(
                name: "TechnicianID",
                table: "JobOrders",
                newName: "TechnicianId");

            migrationBuilder.RenameColumn(
                name: "ClientID",
                table: "JobOrders",
                newName: "ClientId");

            migrationBuilder.RenameIndex(
                name: "IX_JobOrders_TechnicianID",
                table: "JobOrders",
                newName: "IX_JobOrders_TechnicianId");

            migrationBuilder.RenameIndex(
                name: "IX_JobOrders_ClientID",
                table: "JobOrders",
                newName: "IX_JobOrders_ClientId");

            migrationBuilder.RenameColumn(
                name: "ClientID",
                table: "Client",
                newName: "ClientId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DueDate",
                table: "JobOrders",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ClientId",
                table: "JobOrders",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_JobOrders_Client_ClientId",
                table: "JobOrders",
                column: "ClientId",
                principalTable: "Client",
                principalColumn: "ClientId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_JobOrders_Technician_TechnicianId",
                table: "JobOrders",
                column: "TechnicianId",
                principalTable: "Technician",
                principalColumn: "TechnicianId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moonmax.Migrations
{
    /// <inheritdoc />
    public partial class AddClientTypeToClient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClientType",
                table: "Client",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(@"
                UPDATE Client
                SET ClientType = 'Walk-In',
                Name = 'Walk-In Customer'
                WHERE Name LIKE 'Walk-In%'
            ");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientType",
                table: "Client");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMRS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNewFieldForGPSSharing2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "token_sharing",
                table: "gps_sharings",
                newName: "owner_token_sharing");

            migrationBuilder.AddColumn<string>(
                name: "guest_token_sharing",
                table: "gps_sharings",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "guest_token_sharing",
                table: "gps_sharings");

            migrationBuilder.RenameColumn(
                name: "owner_token_sharing",
                table: "gps_sharings",
                newName: "token_sharing");
        }
    }
}

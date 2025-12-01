using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMRS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldForGPSSharing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "token_sharing",
                table: "gps_sharings",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "token_sharing",
                table: "gps_sharings");
        }
    }
}

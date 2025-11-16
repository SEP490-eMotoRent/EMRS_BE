using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMRS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFlespiSupportForVehicle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "protrack_password",
                table: "vehicles",
                newName: "device_imei");

            migrationBuilder.RenameColumn(
                name: "protrack_account",
                table: "vehicles",
                newName: "device_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "device_imei",
                table: "vehicles",
                newName: "protrack_password");

            migrationBuilder.RenameColumn(
                name: "device_id",
                table: "vehicles",
                newName: "protrack_account");
        }
    }
}

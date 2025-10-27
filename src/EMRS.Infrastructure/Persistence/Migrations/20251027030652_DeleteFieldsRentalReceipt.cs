using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMRS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DeleteFieldsRentalReceipt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "battery_percentage",
                table: "rental_receipts");

            migrationBuilder.DropColumn(
                name: "odometer_reading",
                table: "rental_receipts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "battery_percentage",
                table: "rental_receipts",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "odometer_reading",
                table: "rental_receipts",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }
    }
}

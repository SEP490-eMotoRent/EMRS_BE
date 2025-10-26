using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMRS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFieldForRentalReceipt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "receipt_date",
                table: "rental_receipts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "receipt_date",
                table: "rental_receipts",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}

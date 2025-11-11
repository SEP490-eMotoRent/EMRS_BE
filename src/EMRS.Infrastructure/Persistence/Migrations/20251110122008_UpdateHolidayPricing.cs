using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMRS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateHolidayPricing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "effective_from",
                table: "holiday_pricings");

            migrationBuilder.DropColumn(
                name: "effective_to",
                table: "holiday_pricings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "effective_from",
                table: "holiday_pricings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "effective_to",
                table: "holiday_pricings",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}

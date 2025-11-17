using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMRS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFieldForMembership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "free_charging_per_month",
                table: "memberships");

            migrationBuilder.DropColumn(
                name: "free_insurannce_package_fee_per_month",
                table: "memberships");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "free_charging_per_month",
                table: "memberships",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "free_insurannce_package_fee_per_month",
                table: "memberships",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }
    }
}

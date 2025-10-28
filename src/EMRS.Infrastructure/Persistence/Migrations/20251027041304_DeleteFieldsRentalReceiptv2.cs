using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMRS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DeleteFieldsRentalReceiptv2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "contract_number",
                table: "rental_contracts");

            migrationBuilder.DropColumn(
                name: "contract_terms",
                table: "rental_contracts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "contract_number",
                table: "rental_contracts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "contract_terms",
                table: "rental_contracts",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}

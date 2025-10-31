using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMRS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ModifyFieldsInRenterAndConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "face_set_token",
                table: "renters");

            migrationBuilder.DropColumn(
                name: "value1",
                table: "configurations");

            migrationBuilder.RenameColumn(
                name: "value2",
                table: "configurations",
                newName: "value");

            migrationBuilder.AddColumn<int>(
                name: "type",
                table: "configurations",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "type",
                table: "configurations");

            migrationBuilder.RenameColumn(
                name: "value",
                table: "configurations",
                newName: "value2");

            migrationBuilder.AddColumn<string>(
                name: "face_set_token",
                table: "renters",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "value1",
                table: "configurations",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}

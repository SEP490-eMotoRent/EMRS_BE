using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMRS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ModifyFieldsForRenter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "face_verification_id",
                table: "renters",
                newName: "face_token");

            migrationBuilder.AddColumn<string>(
                name: "face_set_token",
                table: "renters",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "face_set_token",
                table: "renters");

            migrationBuilder.RenameColumn(
                name: "face_token",
                table: "renters",
                newName: "face_verification_id");
        }
    }
}

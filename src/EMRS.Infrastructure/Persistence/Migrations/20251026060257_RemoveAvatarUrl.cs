using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMRS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAvatarUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "avatar_url",
                table: "renters");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "avatar_url",
                table: "renters",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}

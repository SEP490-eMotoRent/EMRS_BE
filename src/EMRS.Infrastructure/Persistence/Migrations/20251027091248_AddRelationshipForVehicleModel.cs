using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMRS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRelationshipForVehicleModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "vehicle_model_id",
                table: "vehicle_transfer_requests",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_vehicle_transfer_requests_vehicle_model_id",
                table: "vehicle_transfer_requests",
                column: "vehicle_model_id");

            migrationBuilder.AddForeignKey(
                name: "fk_vehicle_transfer_requests_vehicle_models_vehicle_model_id",
                table: "vehicle_transfer_requests",
                column: "vehicle_model_id",
                principalTable: "vehicle_models",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_vehicle_transfer_requests_vehicle_models_vehicle_model_id",
                table: "vehicle_transfer_requests");

            migrationBuilder.DropIndex(
                name: "ix_vehicle_transfer_requests_vehicle_model_id",
                table: "vehicle_transfer_requests");

            migrationBuilder.DropColumn(
                name: "vehicle_model_id",
                table: "vehicle_transfer_requests");
        }
    }
}

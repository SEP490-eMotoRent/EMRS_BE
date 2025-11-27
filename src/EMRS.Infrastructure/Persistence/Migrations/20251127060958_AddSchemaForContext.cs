using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMRS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSchemaForContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_gps_sharing_renters_guest_renter_id",
                table: "gps_sharing");

            migrationBuilder.DropForeignKey(
                name: "fk_gps_sharing_renters_owner_renter_id",
                table: "gps_sharing");

            migrationBuilder.DropForeignKey(
                name: "fk_gps_sharing_renters_renter_id",
                table: "gps_sharing");

            migrationBuilder.DropForeignKey(
                name: "fk_gps_sharing_vehicles_guest_vehicle_id",
                table: "gps_sharing");

            migrationBuilder.DropForeignKey(
                name: "fk_gps_sharing_vehicles_owner_vehicle_id",
                table: "gps_sharing");

            migrationBuilder.DropPrimaryKey(
                name: "pk_gps_sharing",
                table: "gps_sharing");

            migrationBuilder.RenameTable(
                name: "gps_sharing",
                newName: "gps_sharings");

            migrationBuilder.RenameIndex(
                name: "ix_gps_sharing_renter_id",
                table: "gps_sharings",
                newName: "ix_gps_sharings_renter_id");

            migrationBuilder.RenameIndex(
                name: "ix_gps_sharing_owner_vehicle_id",
                table: "gps_sharings",
                newName: "ix_gps_sharings_owner_vehicle_id");

            migrationBuilder.RenameIndex(
                name: "ix_gps_sharing_owner_renter_id",
                table: "gps_sharings",
                newName: "ix_gps_sharings_owner_renter_id");

            migrationBuilder.RenameIndex(
                name: "ix_gps_sharing_guest_vehicle_id",
                table: "gps_sharings",
                newName: "ix_gps_sharings_guest_vehicle_id");

            migrationBuilder.RenameIndex(
                name: "ix_gps_sharing_guest_renter_id",
                table: "gps_sharings",
                newName: "ix_gps_sharings_guest_renter_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_gps_sharings",
                table: "gps_sharings",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_gps_sharings_renters_guest_renter_id",
                table: "gps_sharings",
                column: "guest_renter_id",
                principalTable: "renters",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_gps_sharings_renters_owner_renter_id",
                table: "gps_sharings",
                column: "owner_renter_id",
                principalTable: "renters",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_gps_sharings_renters_renter_id",
                table: "gps_sharings",
                column: "renter_id",
                principalTable: "renters",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_gps_sharings_vehicles_guest_vehicle_id",
                table: "gps_sharings",
                column: "guest_vehicle_id",
                principalTable: "vehicles",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_gps_sharings_vehicles_owner_vehicle_id",
                table: "gps_sharings",
                column: "owner_vehicle_id",
                principalTable: "vehicles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_gps_sharings_renters_guest_renter_id",
                table: "gps_sharings");

            migrationBuilder.DropForeignKey(
                name: "fk_gps_sharings_renters_owner_renter_id",
                table: "gps_sharings");

            migrationBuilder.DropForeignKey(
                name: "fk_gps_sharings_renters_renter_id",
                table: "gps_sharings");

            migrationBuilder.DropForeignKey(
                name: "fk_gps_sharings_vehicles_guest_vehicle_id",
                table: "gps_sharings");

            migrationBuilder.DropForeignKey(
                name: "fk_gps_sharings_vehicles_owner_vehicle_id",
                table: "gps_sharings");

            migrationBuilder.DropPrimaryKey(
                name: "pk_gps_sharings",
                table: "gps_sharings");

            migrationBuilder.RenameTable(
                name: "gps_sharings",
                newName: "gps_sharing");

            migrationBuilder.RenameIndex(
                name: "ix_gps_sharings_renter_id",
                table: "gps_sharing",
                newName: "ix_gps_sharing_renter_id");

            migrationBuilder.RenameIndex(
                name: "ix_gps_sharings_owner_vehicle_id",
                table: "gps_sharing",
                newName: "ix_gps_sharing_owner_vehicle_id");

            migrationBuilder.RenameIndex(
                name: "ix_gps_sharings_owner_renter_id",
                table: "gps_sharing",
                newName: "ix_gps_sharing_owner_renter_id");

            migrationBuilder.RenameIndex(
                name: "ix_gps_sharings_guest_vehicle_id",
                table: "gps_sharing",
                newName: "ix_gps_sharing_guest_vehicle_id");

            migrationBuilder.RenameIndex(
                name: "ix_gps_sharings_guest_renter_id",
                table: "gps_sharing",
                newName: "ix_gps_sharing_guest_renter_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_gps_sharing",
                table: "gps_sharing",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_gps_sharing_renters_guest_renter_id",
                table: "gps_sharing",
                column: "guest_renter_id",
                principalTable: "renters",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_gps_sharing_renters_owner_renter_id",
                table: "gps_sharing",
                column: "owner_renter_id",
                principalTable: "renters",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_gps_sharing_renters_renter_id",
                table: "gps_sharing",
                column: "renter_id",
                principalTable: "renters",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_gps_sharing_vehicles_guest_vehicle_id",
                table: "gps_sharing",
                column: "guest_vehicle_id",
                principalTable: "vehicles",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_gps_sharing_vehicles_owner_vehicle_id",
                table: "gps_sharing",
                column: "owner_vehicle_id",
                principalTable: "vehicles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

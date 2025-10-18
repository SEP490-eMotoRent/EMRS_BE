using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMRS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "text", nullable: false),
                    password = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<string>(type: "text", nullable: false),
                    fullname = table.Column<string>(type: "text", nullable: true),
                    refresh_token = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    refresh_token_expiry = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_refresh_token_revoked = table.Column<bool>(type: "boolean", nullable: false),
                    reset_password_token = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    reset_password_token_expiry = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_accounts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "branches",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    branch_name = table.Column<string>(type: "text", nullable: false),
                    address = table.Column<string>(type: "text", nullable: false),
                    city = table.Column<string>(type: "text", nullable: false),
                    phone = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: false),
                    longitude = table.Column<double>(type: "double precision", nullable: false),
                    opening_time = table.Column<string>(type: "text", nullable: false),
                    closing_time = table.Column<string>(type: "text", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_branches", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "configurations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    value1 = table.Column<string>(type: "text", nullable: false),
                    value2 = table.Column<string>(type: "text", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_configurations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "holiday_pricings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    holiday_name = table.Column<string>(type: "text", nullable: false),
                    holiday_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    price_multiplier = table.Column<decimal>(type: "numeric", nullable: false),
                    effective_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    effective_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    description = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_holiday_pricings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "insurance_packages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    package_name = table.Column<string>(type: "text", nullable: false),
                    package_fee = table.Column<decimal>(type: "numeric", nullable: false),
                    coverage_person_limit = table.Column<decimal>(type: "numeric", nullable: false),
                    coverage_property_limit = table.Column<decimal>(type: "numeric", nullable: false),
                    coverage_vehicle_percentage = table.Column<decimal>(type: "numeric", nullable: false),
                    coverage_theft = table.Column<decimal>(type: "numeric", nullable: false),
                    deductible_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_insurance_packages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "media",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    media_name = table.Column<string>(type: "text", nullable: false),
                    media_type = table.Column<string>(type: "text", nullable: false),
                    file_url = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    doc_no = table.Column<string>(type: "text", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_media", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "memberships",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tier_name = table.Column<string>(type: "text", nullable: false),
                    min_bookings = table.Column<decimal>(type: "numeric", nullable: false),
                    discount_percentage = table.Column<decimal>(type: "numeric", nullable: false),
                    free_charging_per_month = table.Column<decimal>(type: "numeric", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_memberships", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rental_pricings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    rental_price = table.Column<decimal>(type: "numeric", nullable: false),
                    excess_km_price = table.Column<decimal>(type: "numeric", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rental_pricings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    transaction_type = table.Column<string>(type: "text", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    doc_no = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    transaction_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transactions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "staffs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_staffs", x => x.id);
                    table.ForeignKey(
                        name: "fk_staffs_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_staffs_branches_branch_id",
                        column: x => x.branch_id,
                        principalTable: "branches",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "vehicle_transfer_orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    received_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: false),
                    from_branch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    to_branch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vehicle_transfer_orders", x => x.id);
                    table.ForeignKey(
                        name: "fk_vehicle_transfer_orders_branches_from_branch_id",
                        column: x => x.from_branch_id,
                        principalTable: "branches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_vehicle_transfer_orders_branches_to_branch_id",
                        column: x => x.to_branch_id,
                        principalTable: "branches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "renters",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    phone = table.Column<string>(type: "text", nullable: false),
                    address = table.Column<string>(type: "text", nullable: false),
                    date_of_birth = table.Column<string>(type: "text", nullable: true),
                    avatar_url = table.Column<string>(type: "text", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    membership_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_verified = table.Column<bool>(type: "boolean", nullable: false),
                    verification_code = table.Column<string>(type: "text", nullable: false),
                    verification_code_expiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_renters", x => x.id);
                    table.ForeignKey(
                        name: "fk_renters_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_renters_memberships_membership_id",
                        column: x => x.membership_id,
                        principalTable: "memberships",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_models",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    model_name = table.Column<string>(type: "text", nullable: false),
                    category = table.Column<string>(type: "text", nullable: false),
                    battery_capacity_kwh = table.Column<decimal>(type: "numeric", nullable: false),
                    max_range_km = table.Column<decimal>(type: "numeric", nullable: false),
                    max_speed_kmh = table.Column<decimal>(type: "numeric", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    rental_pricing_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vehicle_models", x => x.id);
                    table.ForeignKey(
                        name: "fk_vehicle_models_rental_pricings_rental_pricing_id",
                        column: x => x.rental_pricing_id,
                        principalTable: "rental_pricings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_transfer_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    quantity_requested = table.Column<decimal>(type: "numeric", nullable: false),
                    requested_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    reviewed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    vehicle_transfer_order_id = table.Column<Guid>(type: "uuid", nullable: true),
                    staff_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vehicle_transfer_requests", x => x.id);
                    table.ForeignKey(
                        name: "fk_vehicle_transfer_requests_staffs_staff_id",
                        column: x => x.staff_id,
                        principalTable: "staffs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_vehicle_transfer_requests_vehicle_transfer_orders_vehicle_t",
                        column: x => x.vehicle_transfer_order_id,
                        principalTable: "vehicle_transfer_orders",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "documents",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_type = table.Column<string>(type: "text", nullable: false),
                    document_number = table.Column<string>(type: "text", nullable: false),
                    issue_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expiry_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    issuing_authority = table.Column<string>(type: "text", nullable: true),
                    verification_status = table.Column<string>(type: "text", nullable: false),
                    verified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    renter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_documents", x => x.id);
                    table.ForeignKey(
                        name: "fk_documents_renters_renter_id",
                        column: x => x.renter_id,
                        principalTable: "renters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "wallets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    balance = table.Column<decimal>(type: "numeric", nullable: false),
                    renter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_wallets", x => x.id);
                    table.ForeignKey(
                        name: "fk_wallets_renters_renter_id",
                        column: x => x.renter_id,
                        principalTable: "renters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vehicles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    license_plate = table.Column<string>(type: "text", nullable: false),
                    color = table.Column<string>(type: "text", nullable: false),
                    year_of_manufacture = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    current_odometer_km = table.Column<decimal>(type: "numeric", nullable: false),
                    battery_health_percentage = table.Column<decimal>(type: "numeric", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    last_maintenance_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    next_maintenance_due = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    purchase_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    description = table.Column<string>(type: "text", nullable: false),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_model_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vehicles", x => x.id);
                    table.ForeignKey(
                        name: "fk_vehicles_branches_branch_id",
                        column: x => x.branch_id,
                        principalTable: "branches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_vehicles_vehicle_models_vehicle_model_id",
                        column: x => x.vehicle_model_id,
                        principalTable: "vehicle_models",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "withdrawal_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    bank_name = table.Column<string>(type: "text", nullable: false),
                    bank_account_number = table.Column<string>(type: "text", nullable: false),
                    bank_account_name = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    rejection_reason = table.Column<string>(type: "text", nullable: false),
                    wallet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_withdrawal_requests", x => x.id);
                    table.ForeignKey(
                        name: "fk_withdrawal_requests_wallets_wallet_id",
                        column: x => x.wallet_id,
                        principalTable: "wallets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "bookings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    start_datetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    end_datetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    actual_return_datetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    base_rental_fee = table.Column<decimal>(type: "numeric", nullable: false),
                    deposit_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    rental_days = table.Column<decimal>(type: "numeric", nullable: false),
                    rental_hours = table.Column<decimal>(type: "numeric", nullable: false),
                    renting_rate = table.Column<decimal>(type: "numeric", nullable: false),
                    late_return_fee = table.Column<decimal>(type: "numeric", nullable: false),
                    average_rental_price = table.Column<decimal>(type: "numeric", nullable: false),
                    excess_km_fee = table.Column<decimal>(type: "numeric", nullable: false),
                    cleaning_fee = table.Column<decimal>(type: "numeric", nullable: false),
                    cross_branch_fee = table.Column<decimal>(type: "numeric", nullable: false),
                    total_charging_fee = table.Column<decimal>(type: "numeric", nullable: false),
                    total_additional_fee = table.Column<decimal>(type: "numeric", nullable: false),
                    total_rental_fee = table.Column<decimal>(type: "numeric", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    refund_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    booking_status = table.Column<string>(type: "text", nullable: false),
                    renter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    insurance_package_id = table.Column<Guid>(type: "uuid", nullable: true),
                    handover_branch_id = table.Column<Guid>(type: "uuid", nullable: true),
                    return_branch_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bookings", x => x.id);
                    table.ForeignKey(
                        name: "fk_bookings_branches_handover_branch_id",
                        column: x => x.handover_branch_id,
                        principalTable: "branches",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_bookings_branches_return_branch_id",
                        column: x => x.return_branch_id,
                        principalTable: "branches",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_bookings_insurance_packages_insurance_package_id",
                        column: x => x.insurance_package_id,
                        principalTable: "insurance_packages",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_bookings_renters_renter_id",
                        column: x => x.renter_id,
                        principalTable: "renters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_bookings_vehicles_vehicle_id",
                        column: x => x.vehicle_id,
                        principalTable: "vehicles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintenance_schedules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    schedule_title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    frequency_type = table.Column<string>(type: "text", nullable: false),
                    frequency_value_km = table.Column<decimal>(type: "numeric", nullable: false),
                    frequency_value_days = table.Column<decimal>(type: "numeric", nullable: false),
                    checklist = table.Column<string>(type: "text", nullable: false),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    staff_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_maintenance_schedules", x => x.id);
                    table.ForeignKey(
                        name: "fk_maintenance_schedules_staffs_staff_id",
                        column: x => x.staff_id,
                        principalTable: "staffs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_maintenance_schedules_vehicles_vehicle_id",
                        column: x => x.vehicle_id,
                        principalTable: "vehicles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "repair_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    issue_description = table.Column<string>(type: "text", nullable: false),
                    priority = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    approved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    staff_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_repair_requests", x => x.id);
                    table.ForeignKey(
                        name: "fk_repair_requests_staffs_staff_id",
                        column: x => x.staff_id,
                        principalTable: "staffs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_repair_requests_vehicles_vehicle_id",
                        column: x => x.vehicle_id,
                        principalTable: "vehicles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "additional_fees",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fee_type = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_additional_fees", x => x.id);
                    table.ForeignKey(
                        name: "fk_additional_fees_bookings_booking_id",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "charging_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    charging_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    start_battery_percentage = table.Column<decimal>(type: "numeric", nullable: false),
                    end_battery_percentage = table.Column<decimal>(type: "numeric", nullable: false),
                    kwh_charged = table.Column<decimal>(type: "numeric", nullable: false),
                    rate_per_kwh = table.Column<decimal>(type: "numeric", nullable: false),
                    fee = table.Column<decimal>(type: "numeric", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: false),
                    booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    staff_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_charging_records", x => x.id);
                    table.ForeignKey(
                        name: "fk_charging_records_bookings_booking_id",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_charging_records_branches_branch_id",
                        column: x => x.branch_id,
                        principalTable: "branches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_charging_records_staffs_staff_id",
                        column: x => x.staff_id,
                        principalTable: "staffs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "feedbacks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    rating = table.Column<decimal>(type: "numeric", nullable: false),
                    comment = table.Column<string>(type: "text", nullable: false),
                    renter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_feedbacks", x => x.id);
                    table.ForeignKey(
                        name: "fk_feedbacks_bookings_booking_id",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_feedbacks_renters_renter_id",
                        column: x => x.renter_id,
                        principalTable: "renters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "insurance_claims",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    incident_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    incident_location = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    severity = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    vehicle_damage_cost = table.Column<decimal>(type: "numeric", nullable: false),
                    person_injury_cost = table.Column<decimal>(type: "numeric", nullable: false),
                    third_party_cost = table.Column<decimal>(type: "numeric", nullable: false),
                    total_cost = table.Column<decimal>(type: "numeric", nullable: false),
                    insurance_coverage_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    renter_liability_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    reviewed_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    rejection_reason = table.Column<string>(type: "text", nullable: false),
                    insurance_claim_pdf_url = table.Column<string>(type: "text", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: false),
                    renter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_insurance_claims", x => x.id);
                    table.ForeignKey(
                        name: "fk_insurance_claims_bookings_booking_id",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_insurance_claims_renters_renter_id",
                        column: x => x.renter_id,
                        principalTable: "renters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rental_contracts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    contract_number = table.Column<string>(type: "text", nullable: false),
                    contract_terms = table.Column<string>(type: "text", nullable: false),
                    otp_code = table.Column<string>(type: "text", nullable: false),
                    expire_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    contract_status = table.Column<string>(type: "text", nullable: false),
                    contract_pdf_url = table.Column<string>(type: "text", nullable: false),
                    booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rental_contracts", x => x.id);
                    table.ForeignKey(
                        name: "fk_rental_contracts_bookings_booking_id",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rental_receipts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    receipt_type = table.Column<string>(type: "text", nullable: false),
                    receipt_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    odometer_reading = table.Column<decimal>(type: "numeric", nullable: false),
                    battery_percentage = table.Column<decimal>(type: "numeric", nullable: false),
                    checklist_json = table.Column<string>(type: "text", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    renter_confirmed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    start_odometer_km = table.Column<decimal>(type: "numeric", nullable: false),
                    end_odometer_km = table.Column<decimal>(type: "numeric", nullable: false),
                    start_battery_percentage = table.Column<decimal>(type: "numeric", nullable: false),
                    end_battery_percentage = table.Column<decimal>(type: "numeric", nullable: false),
                    booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    staff_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rental_receipts", x => x.id);
                    table.ForeignKey(
                        name: "fk_rental_receipts_bookings_booking_id",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_rental_receipts_staffs_staff_id",
                        column: x => x.staff_id,
                        principalTable: "staffs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintenance_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    maintenance_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    issues_found = table.Column<string>(type: "text", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    staff_id = table.Column<Guid>(type: "uuid", nullable: false),
                    maintenance_schedule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_maintenance_records", x => x.id);
                    table.ForeignKey(
                        name: "fk_maintenance_records_maintenance_schedules_maintenance_sched",
                        column: x => x.maintenance_schedule_id,
                        principalTable: "maintenance_schedules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_maintenance_records_staffs_staff_id",
                        column: x => x.staff_id,
                        principalTable: "staffs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_additional_fees_booking_id",
                table: "additional_fees",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "ix_bookings_handover_branch_id",
                table: "bookings",
                column: "handover_branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_bookings_insurance_package_id",
                table: "bookings",
                column: "insurance_package_id");

            migrationBuilder.CreateIndex(
                name: "ix_bookings_renter_id",
                table: "bookings",
                column: "renter_id");

            migrationBuilder.CreateIndex(
                name: "ix_bookings_return_branch_id",
                table: "bookings",
                column: "return_branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_bookings_vehicle_id",
                table: "bookings",
                column: "vehicle_id");

            migrationBuilder.CreateIndex(
                name: "ix_charging_records_booking_id",
                table: "charging_records",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "ix_charging_records_branch_id",
                table: "charging_records",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_charging_records_staff_id",
                table: "charging_records",
                column: "staff_id");

            migrationBuilder.CreateIndex(
                name: "ix_documents_renter_id",
                table: "documents",
                column: "renter_id");

            migrationBuilder.CreateIndex(
                name: "ix_feedbacks_booking_id",
                table: "feedbacks",
                column: "booking_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_feedbacks_renter_id",
                table: "feedbacks",
                column: "renter_id");

            migrationBuilder.CreateIndex(
                name: "ix_insurance_claims_booking_id",
                table: "insurance_claims",
                column: "booking_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_insurance_claims_renter_id",
                table: "insurance_claims",
                column: "renter_id");

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_records_maintenance_schedule_id",
                table: "maintenance_records",
                column: "maintenance_schedule_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_records_staff_id",
                table: "maintenance_records",
                column: "staff_id");

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_schedules_staff_id",
                table: "maintenance_schedules",
                column: "staff_id");

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_schedules_vehicle_id",
                table: "maintenance_schedules",
                column: "vehicle_id");

            migrationBuilder.CreateIndex(
                name: "ix_rental_contracts_booking_id",
                table: "rental_contracts",
                column: "booking_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_rental_receipts_booking_id",
                table: "rental_receipts",
                column: "booking_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_rental_receipts_staff_id",
                table: "rental_receipts",
                column: "staff_id");

            migrationBuilder.CreateIndex(
                name: "ix_renters_account_id",
                table: "renters",
                column: "account_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_renters_membership_id",
                table: "renters",
                column: "membership_id");

            migrationBuilder.CreateIndex(
                name: "ix_repair_requests_staff_id",
                table: "repair_requests",
                column: "staff_id");

            migrationBuilder.CreateIndex(
                name: "ix_repair_requests_vehicle_id",
                table: "repair_requests",
                column: "vehicle_id");

            migrationBuilder.CreateIndex(
                name: "ix_staffs_account_id",
                table: "staffs",
                column: "account_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_staffs_branch_id",
                table: "staffs",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_vehicle_models_rental_pricing_id",
                table: "vehicle_models",
                column: "rental_pricing_id");

            migrationBuilder.CreateIndex(
                name: "ix_vehicle_transfer_orders_from_branch_id",
                table: "vehicle_transfer_orders",
                column: "from_branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_vehicle_transfer_orders_to_branch_id",
                table: "vehicle_transfer_orders",
                column: "to_branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_vehicle_transfer_requests_staff_id",
                table: "vehicle_transfer_requests",
                column: "staff_id");

            migrationBuilder.CreateIndex(
                name: "ix_vehicle_transfer_requests_vehicle_transfer_order_id",
                table: "vehicle_transfer_requests",
                column: "vehicle_transfer_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_vehicles_branch_id",
                table: "vehicles",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_vehicles_vehicle_model_id",
                table: "vehicles",
                column: "vehicle_model_id");

            migrationBuilder.CreateIndex(
                name: "ix_wallets_renter_id",
                table: "wallets",
                column: "renter_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_withdrawal_requests_wallet_id",
                table: "withdrawal_requests",
                column: "wallet_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "additional_fees");

            migrationBuilder.DropTable(
                name: "charging_records");

            migrationBuilder.DropTable(
                name: "configurations");

            migrationBuilder.DropTable(
                name: "documents");

            migrationBuilder.DropTable(
                name: "feedbacks");

            migrationBuilder.DropTable(
                name: "holiday_pricings");

            migrationBuilder.DropTable(
                name: "insurance_claims");

            migrationBuilder.DropTable(
                name: "maintenance_records");

            migrationBuilder.DropTable(
                name: "media");

            migrationBuilder.DropTable(
                name: "rental_contracts");

            migrationBuilder.DropTable(
                name: "rental_receipts");

            migrationBuilder.DropTable(
                name: "repair_requests");

            migrationBuilder.DropTable(
                name: "transactions");

            migrationBuilder.DropTable(
                name: "vehicle_transfer_requests");

            migrationBuilder.DropTable(
                name: "withdrawal_requests");

            migrationBuilder.DropTable(
                name: "maintenance_schedules");

            migrationBuilder.DropTable(
                name: "bookings");

            migrationBuilder.DropTable(
                name: "vehicle_transfer_orders");

            migrationBuilder.DropTable(
                name: "wallets");

            migrationBuilder.DropTable(
                name: "staffs");

            migrationBuilder.DropTable(
                name: "insurance_packages");

            migrationBuilder.DropTable(
                name: "vehicles");

            migrationBuilder.DropTable(
                name: "renters");

            migrationBuilder.DropTable(
                name: "branches");

            migrationBuilder.DropTable(
                name: "vehicle_models");

            migrationBuilder.DropTable(
                name: "accounts");

            migrationBuilder.DropTable(
                name: "memberships");

            migrationBuilder.DropTable(
                name: "rental_pricings");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HvacConnect.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Staging");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:citext", ",,")
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,");

            migrationBuilder.CreateSequence<int>(
                name: "seq_asset_tag",
                schema: "Staging",
                startValue: 100L);

            migrationBuilder.CreateSequence<int>(
                name: "seq_customer_code",
                schema: "Staging",
                startValue: 100L);

            migrationBuilder.CreateSequence<int>(
                name: "seq_duplicate_code",
                schema: "Staging");

            migrationBuilder.CreateSequence<int>(
                name: "seq_import_job",
                schema: "Staging",
                startValue: 200L);

            migrationBuilder.CreateSequence<int>(
                name: "seq_site_code",
                schema: "Staging",
                startValue: 100L);

            migrationBuilder.CreateSequence<int>(
                name: "seq_ticket_number",
                schema: "Staging",
                startValue: 2400L);

            migrationBuilder.CreateSequence<int>(
                name: "seq_vendor_code",
                schema: "Staging");

            migrationBuilder.CreateTable(
                name: "canonical_customers",
                schema: "Staging",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    canonical_code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    customer_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    normalized_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    customer_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    primary_contact_name = table.Column<string>(type: "text", nullable: true),
                    primary_contact_email = table.Column<string>(type: "text", nullable: true),
                    primary_contact_phone = table.Column<string>(type: "text", nullable: true),
                    billing_address = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_self_registered = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_canonical_customers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vendors",
                schema: "Staging",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    vendor_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    brand_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    legal_entity_name = table.Column<string>(type: "text", nullable: true),
                    region = table.Column<string>(type: "text", nullable: false),
                    primary_contact_name = table.Column<string>(type: "text", nullable: true),
                    primary_contact_email = table.Column<string>(type: "text", nullable: true),
                    primary_contact_phone = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    onboarding_phase = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    phase1approved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    phase1approved_by_user_id = table.Column<int>(type: "integer", nullable: true),
                    cutover_date = table.Column<DateOnly>(type: "date", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_by_user_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vendors", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "brand_customer_accounts",
                schema: "Staging",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    canonical_customer_id = table.Column<int>(type: "integer", nullable: false),
                    vendor_id = table.Column<int>(type: "integer", nullable: false),
                    local_account_number = table.Column<string>(type: "text", nullable: false),
                    local_customer_name = table.Column<string>(type: "text", nullable: false),
                    contact_email = table.Column<string>(type: "text", nullable: true),
                    contact_phone = table.Column<string>(type: "text", nullable: true),
                    billing_address = table.Column<string>(type: "text", nullable: true),
                    account_status = table.Column<string>(type: "text", nullable: false),
                    source_system = table.Column<string>(type: "text", nullable: true),
                    source_system_key = table.Column<string>(type: "text", nullable: true),
                    row_hash = table.Column<string>(type: "text", nullable: true),
                    first_import_job_id = table.Column<int>(type: "integer", nullable: true),
                    last_import_job_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_brand_customer_accounts", x => x.id);
                    table.ForeignKey(
                        name: "fk_brand_customer_accounts_canonical_customers_canonical_custo",
                        column: x => x.canonical_customer_id,
                        principalSchema: "Staging",
                        principalTable: "canonical_customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_brand_customer_accounts_vendors_vendor_id",
                        column: x => x.vendor_id,
                        principalSchema: "Staging",
                        principalTable: "vendors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "import_jobs",
                schema: "Staging",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    job_code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    vendor_id = table.Column<int>(type: "integer", nullable: false),
                    entity_type = table.Column<string>(type: "text", nullable: false),
                    batch_description = table.Column<string>(type: "text", nullable: true),
                    original_file_name = table.Column<string>(type: "text", nullable: false),
                    file_size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    file_hash = table.Column<string>(type: "text", nullable: false),
                    uploaded_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    current_stage = table.Column<short>(type: "smallint", nullable: false),
                    total_rows = table.Column<int>(type: "integer", nullable: false),
                    valid_rows = table.Column<int>(type: "integer", nullable: false),
                    warning_rows = table.Column<int>(type: "integer", nullable: false),
                    error_rows = table.Column<int>(type: "integer", nullable: false),
                    duplicate_rows = table.Column<int>(type: "integer", nullable: false),
                    updated_rows = table.Column<int>(type: "integer", nullable: false),
                    skipped_rows = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    mapping_config_json = table.Column<string>(type: "text", nullable: true),
                    failure_reason = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_import_jobs", x => x.id);
                    table.ForeignKey(
                        name: "fk_import_jobs_vendors_vendor_id",
                        column: x => x.vendor_id,
                        principalSchema: "Staging",
                        principalTable: "vendors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "site_locations",
                schema: "Staging",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    site_code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    canonical_customer_id = table.Column<int>(type: "integer", nullable: false),
                    primary_vendor_id = table.Column<int>(type: "integer", nullable: true),
                    parent_site_id = table.Column<int>(type: "integer", nullable: true),
                    site_name = table.Column<string>(type: "text", nullable: false),
                    site_type = table.Column<string>(type: "text", nullable: false),
                    address_line = table.Column<string>(type: "text", nullable: false),
                    city = table.Column<string>(type: "text", nullable: false),
                    state = table.Column<string>(type: "text", nullable: false),
                    postal_code = table.Column<string>(type: "text", nullable: false),
                    time_zone = table.Column<string>(type: "text", nullable: false),
                    operating_hours = table.Column<string>(type: "text", nullable: true),
                    access_instructions = table.Column<string>(type: "text", nullable: true),
                    site_contact_name = table.Column<string>(type: "text", nullable: true),
                    site_contact_phone = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_site_locations", x => x.id);
                    table.ForeignKey(
                        name: "fk_site_locations_canonical_customers_canonical_customer_id",
                        column: x => x.canonical_customer_id,
                        principalSchema: "Staging",
                        principalTable: "canonical_customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_site_locations_site_locations_parent_site_id",
                        column: x => x.parent_site_id,
                        principalSchema: "Staging",
                        principalTable: "site_locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_site_locations_vendors_primary_vendor_id",
                        column: x => x.primary_vendor_id,
                        principalSchema: "Staging",
                        principalTable: "vendors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "Staging",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    email = table.Column<string>(type: "citext", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    first_name = table.Column<string>(type: "text", nullable: false),
                    last_name = table.Column<string>(type: "text", nullable: false),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    role = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    vendor_id = table.Column<int>(type: "integer", nullable: true),
                    customer_id = table.Column<int>(type: "integer", nullable: true),
                    employee_code = table.Column<string>(type: "text", nullable: true),
                    specialisation = table.Column<string>(type: "text", nullable: true),
                    availability_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    failed_login_count = table.Column<int>(type: "integer", nullable: false),
                    lockout_end_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                    table.ForeignKey(
                        name: "fk_users_canonical_customers_customer_id",
                        column: x => x.customer_id,
                        principalSchema: "Staging",
                        principalTable: "canonical_customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_users_vendors_vendor_id",
                        column: x => x.vendor_id,
                        principalSchema: "Staging",
                        principalTable: "vendors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vendor_mapping_templates",
                schema: "Staging",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    vendor_id = table.Column<int>(type: "integer", nullable: false),
                    entity_type = table.Column<string>(type: "text", nullable: false),
                    template_name = table.Column<string>(type: "text", nullable: false),
                    mapping_config_json = table.Column<string>(type: "text", nullable: false),
                    created_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vendor_mapping_templates", x => x.id);
                    table.ForeignKey(
                        name: "fk_vendor_mapping_templates_vendors_vendor_id",
                        column: x => x.vendor_id,
                        principalSchema: "Staging",
                        principalTable: "vendors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vendor_onboarding_checklists",
                schema: "Staging",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    vendor_id = table.Column<int>(type: "integer", nullable: false),
                    phase = table.Column<string>(type: "text", nullable: false),
                    task_description = table.Column<string>(type: "text", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false),
                    completed_by_user_id = table.Column<int>(type: "integer", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vendor_onboarding_checklists", x => x.id);
                    table.ForeignKey(
                        name: "fk_vendor_onboarding_checklists_vendors_vendor_id",
                        column: x => x.vendor_id,
                        principalSchema: "Staging",
                        principalTable: "vendors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "import_rows",
                schema: "Staging",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    import_job_id = table.Column<int>(type: "integer", nullable: false),
                    row_number = table.Column<int>(type: "integer", nullable: false),
                    raw_data_json = table.Column<string>(type: "text", nullable: false),
                    mapped_data_json = table.Column<string>(type: "text", nullable: true),
                    normalized_name = table.Column<string>(type: "text", nullable: true),
                    row_hash = table.Column<string>(type: "text", nullable: true),
                    validation_status = table.Column<string>(type: "text", nullable: false),
                    validation_issues_json = table.Column<string>(type: "text", nullable: true),
                    match_outcome = table.Column<string>(type: "text", nullable: true),
                    resolved_canonical_customer_id = table.Column<int>(type: "integer", nullable: true),
                    is_processed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_import_rows", x => x.id);
                    table.ForeignKey(
                        name: "fk_import_rows_import_jobs_import_job_id",
                        column: x => x.import_job_id,
                        principalSchema: "Staging",
                        principalTable: "import_jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "asset_equipment",
                schema: "Staging",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    asset_tag = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    site_location_id = table.Column<int>(type: "integer", nullable: false),
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    manufacturer = table.Column<string>(type: "text", nullable: false),
                    model_number = table.Column<string>(type: "text", nullable: true),
                    serial_number = table.Column<string>(type: "text", nullable: true),
                    refrigerant_type = table.Column<string>(type: "text", nullable: true),
                    install_date = table.Column<DateOnly>(type: "date", nullable: true),
                    warranty_end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    source_vendor_id = table.Column<int>(type: "integer", nullable: true),
                    legacy_asset_id = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asset_equipment", x => x.id);
                    table.ForeignKey(
                        name: "fk_asset_equipment_site_locations_site_location_id",
                        column: x => x.site_location_id,
                        principalSchema: "Staging",
                        principalTable: "site_locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "site_location_histories",
                schema: "Staging",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    site_location_id = table.Column<int>(type: "integer", nullable: false),
                    changed_by_user_id = table.Column<int>(type: "integer", nullable: true),
                    change_type = table.Column<string>(type: "text", nullable: false),
                    previous_values_json = table.Column<string>(type: "text", nullable: true),
                    new_values_json = table.Column<string>(type: "text", nullable: true),
                    reason_notes = table.Column<string>(type: "text", nullable: true),
                    effective_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_site_location_histories", x => x.id);
                    table.ForeignKey(
                        name: "fk_site_location_histories_site_locations_site_location_id",
                        column: x => x.site_location_id,
                        principalSchema: "Staging",
                        principalTable: "site_locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "duplicate_candidates",
                schema: "Staging",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    duplicate_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    import_job_id = table.Column<int>(type: "integer", nullable: false),
                    import_row_id = table.Column<long>(type: "bigint", nullable: false),
                    entity_type = table.Column<string>(type: "text", nullable: false),
                    candidate_vendor_id = table.Column<int>(type: "integer", nullable: false),
                    candidate_name = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: true),
                    phone = table.Column<string>(type: "text", nullable: true),
                    account_number = table.Column<string>(type: "text", nullable: true),
                    address = table.Column<string>(type: "text", nullable: true),
                    existing_canonical_customer_id = table.Column<int>(type: "integer", nullable: false),
                    confidence_score = table.Column<short>(type: "smallint", nullable: false),
                    confidence_level = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    match_reason = table.Column<string>(type: "text", nullable: false),
                    decision = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    review_notes = table.Column<string>(type: "text", nullable: true),
                    reviewed_by_user_id = table.Column<int>(type: "integer", nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_duplicate_candidates", x => x.id);
                    table.ForeignKey(
                        name: "fk_duplicate_candidates_canonical_customers_existing_canonical",
                        column: x => x.existing_canonical_customer_id,
                        principalSchema: "Staging",
                        principalTable: "canonical_customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_duplicate_candidates_import_jobs_import_job_id",
                        column: x => x.import_job_id,
                        principalSchema: "Staging",
                        principalTable: "import_jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_duplicate_candidates_import_rows_import_row_id",
                        column: x => x.import_row_id,
                        principalSchema: "Staging",
                        principalTable: "import_rows",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "service_tickets",
                schema: "Staging",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ticket_number = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    canonical_customer_id = table.Column<int>(type: "integer", nullable: false),
                    site_location_id = table.Column<int>(type: "integer", nullable: false),
                    vendor_id = table.Column<int>(type: "integer", nullable: true),
                    asset_equipment_id = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    raised_by_user_id = table.Column<int>(type: "integer", nullable: true),
                    owner_coordinator_id = table.Column<int>(type: "integer", nullable: true),
                    claimed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    assigned_technician_id = table.Column<int>(type: "integer", nullable: true),
                    assigned_by_user_id = table.Column<int>(type: "integer", nullable: true),
                    assigned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    scheduled_start_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    responded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    target_response_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    target_resolution_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    sla_state = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    resolved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    closed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    resolution_notes = table.Column<string>(type: "text", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_tickets", x => x.id);
                    table.ForeignKey(
                        name: "fk_service_tickets_asset_equipment_asset_equipment_id",
                        column: x => x.asset_equipment_id,
                        principalSchema: "Staging",
                        principalTable: "asset_equipment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_service_tickets_canonical_customers_canonical_customer_id",
                        column: x => x.canonical_customer_id,
                        principalSchema: "Staging",
                        principalTable: "canonical_customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_service_tickets_site_locations_site_location_id",
                        column: x => x.site_location_id,
                        principalSchema: "Staging",
                        principalTable: "site_locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_service_tickets_users_assigned_by_user_id",
                        column: x => x.assigned_by_user_id,
                        principalSchema: "Staging",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_service_tickets_users_assigned_technician_id",
                        column: x => x.assigned_technician_id,
                        principalSchema: "Staging",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_service_tickets_users_owner_coordinator_id",
                        column: x => x.owner_coordinator_id,
                        principalSchema: "Staging",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_service_tickets_users_raised_by_user_id",
                        column: x => x.raised_by_user_id,
                        principalSchema: "Staging",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_service_tickets_vendors_vendor_id",
                        column: x => x.vendor_id,
                        principalSchema: "Staging",
                        principalTable: "vendors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ticket_comments",
                schema: "Staging",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ticket_id = table.Column<int>(type: "integer", nullable: false),
                    author_user_id = table.Column<int>(type: "integer", nullable: false),
                    comment_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    comment_text = table.Column<string>(type: "text", nullable: false),
                    is_internal_only = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ticket_comments", x => x.id);
                    table.ForeignKey(
                        name: "fk_ticket_comments_service_tickets_ticket_id",
                        column: x => x.ticket_id,
                        principalSchema: "Staging",
                        principalTable: "service_tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_ticket_comments_users_author_user_id",
                        column: x => x.author_user_id,
                        principalSchema: "Staging",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_asset_equipment_asset_tag",
                schema: "Staging",
                table: "asset_equipment",
                column: "asset_tag",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_asset_equipment_site_location_id",
                schema: "Staging",
                table: "asset_equipment",
                column: "site_location_id");

            migrationBuilder.CreateIndex(
                name: "ix_asset_equipment_source_vendor_id_legacy_asset_id",
                schema: "Staging",
                table: "asset_equipment",
                columns: new[] { "source_vendor_id", "legacy_asset_id" },
                unique: true,
                filter: "legacy_asset_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_brand_customer_accounts_canonical_customer_id",
                schema: "Staging",
                table: "brand_customer_accounts",
                column: "canonical_customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_brand_customer_accounts_vendor_id_local_account_number",
                schema: "Staging",
                table: "brand_customer_accounts",
                columns: new[] { "vendor_id", "local_account_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_canonical_customers_canonical_code",
                schema: "Staging",
                table: "canonical_customers",
                column: "canonical_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_canonical_customers_normalized_name",
                schema: "Staging",
                table: "canonical_customers",
                column: "normalized_name")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_duplicate_candidates_decision_confidence_score",
                schema: "Staging",
                table: "duplicate_candidates",
                columns: new[] { "decision", "confidence_score" },
                filter: "decision = 'PendingReview'");

            migrationBuilder.CreateIndex(
                name: "ix_duplicate_candidates_duplicate_code",
                schema: "Staging",
                table: "duplicate_candidates",
                column: "duplicate_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_duplicate_candidates_existing_canonical_customer_id",
                schema: "Staging",
                table: "duplicate_candidates",
                column: "existing_canonical_customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_duplicate_candidates_import_job_id",
                schema: "Staging",
                table: "duplicate_candidates",
                column: "import_job_id");

            migrationBuilder.CreateIndex(
                name: "ix_duplicate_candidates_import_row_id",
                schema: "Staging",
                table: "duplicate_candidates",
                column: "import_row_id");

            migrationBuilder.CreateIndex(
                name: "ix_import_jobs_job_code",
                schema: "Staging",
                table: "import_jobs",
                column: "job_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_import_jobs_vendor_id",
                schema: "Staging",
                table: "import_jobs",
                column: "vendor_id");

            migrationBuilder.CreateIndex(
                name: "ix_import_rows_import_job_id_row_number",
                schema: "Staging",
                table: "import_rows",
                columns: new[] { "import_job_id", "row_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_service_tickets_asset_equipment_id",
                schema: "Staging",
                table: "service_tickets",
                column: "asset_equipment_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_tickets_assigned_by_user_id",
                schema: "Staging",
                table: "service_tickets",
                column: "assigned_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_tickets_assigned_technician_id",
                schema: "Staging",
                table: "service_tickets",
                column: "assigned_technician_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_tickets_canonical_customer_id",
                schema: "Staging",
                table: "service_tickets",
                column: "canonical_customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_tickets_raised_by_user_id",
                schema: "Staging",
                table: "service_tickets",
                column: "raised_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_tickets_site_location_id",
                schema: "Staging",
                table: "service_tickets",
                column: "site_location_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_tickets_sla_state",
                schema: "Staging",
                table: "service_tickets",
                column: "sla_state");

            migrationBuilder.CreateIndex(
                name: "ix_service_tickets_target_resolution_at",
                schema: "Staging",
                table: "service_tickets",
                column: "target_resolution_at");

            migrationBuilder.CreateIndex(
                name: "ix_service_tickets_ticket_number",
                schema: "Staging",
                table: "service_tickets",
                column: "ticket_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_service_tickets_unclaimed_open_queue",
                schema: "Staging",
                table: "service_tickets",
                columns: new[] { "owner_coordinator_id", "status" },
                filter: "owner_coordinator_id IS NULL AND status = 'Open'");

            migrationBuilder.CreateIndex(
                name: "ix_service_tickets_vendor_id_status",
                schema: "Staging",
                table: "service_tickets",
                columns: new[] { "vendor_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_site_location_histories_site_location_id",
                schema: "Staging",
                table: "site_location_histories",
                column: "site_location_id");

            migrationBuilder.CreateIndex(
                name: "ix_site_locations_canonical_customer_id",
                schema: "Staging",
                table: "site_locations",
                column: "canonical_customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_site_locations_parent_site_id",
                schema: "Staging",
                table: "site_locations",
                column: "parent_site_id");

            migrationBuilder.CreateIndex(
                name: "ix_site_locations_primary_vendor_id",
                schema: "Staging",
                table: "site_locations",
                column: "primary_vendor_id");

            migrationBuilder.CreateIndex(
                name: "ix_site_locations_site_code",
                schema: "Staging",
                table: "site_locations",
                column: "site_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_ticket_comments_author_user_id",
                schema: "Staging",
                table: "ticket_comments",
                column: "author_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_ticket_comments_ticket_id_created_at",
                schema: "Staging",
                table: "ticket_comments",
                columns: new[] { "ticket_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_users_customer_id",
                schema: "Staging",
                table: "users",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                schema: "Staging",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_vendor_id",
                schema: "Staging",
                table: "users",
                column: "vendor_id");

            migrationBuilder.CreateIndex(
                name: "ix_vendor_mapping_templates_vendor_id_entity_type_template_name",
                schema: "Staging",
                table: "vendor_mapping_templates",
                columns: new[] { "vendor_id", "entity_type", "template_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vendor_onboarding_checklists_vendor_id",
                schema: "Staging",
                table: "vendor_onboarding_checklists",
                column: "vendor_id");

            migrationBuilder.CreateIndex(
                name: "ix_vendors_vendor_code",
                schema: "Staging",
                table: "vendors",
                column: "vendor_code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "brand_customer_accounts",
                schema: "Staging");

            migrationBuilder.DropTable(
                name: "duplicate_candidates",
                schema: "Staging");

            migrationBuilder.DropTable(
                name: "site_location_histories",
                schema: "Staging");

            migrationBuilder.DropTable(
                name: "ticket_comments",
                schema: "Staging");

            migrationBuilder.DropTable(
                name: "vendor_mapping_templates",
                schema: "Staging");

            migrationBuilder.DropTable(
                name: "vendor_onboarding_checklists",
                schema: "Staging");

            migrationBuilder.DropTable(
                name: "import_rows",
                schema: "Staging");

            migrationBuilder.DropTable(
                name: "service_tickets",
                schema: "Staging");

            migrationBuilder.DropTable(
                name: "import_jobs",
                schema: "Staging");

            migrationBuilder.DropTable(
                name: "asset_equipment",
                schema: "Staging");

            migrationBuilder.DropTable(
                name: "users",
                schema: "Staging");

            migrationBuilder.DropTable(
                name: "site_locations",
                schema: "Staging");

            migrationBuilder.DropTable(
                name: "canonical_customers",
                schema: "Staging");

            migrationBuilder.DropTable(
                name: "vendors",
                schema: "Staging");

            migrationBuilder.DropSequence(
                name: "seq_asset_tag",
                schema: "Staging");

            migrationBuilder.DropSequence(
                name: "seq_customer_code",
                schema: "Staging");

            migrationBuilder.DropSequence(
                name: "seq_duplicate_code",
                schema: "Staging");

            migrationBuilder.DropSequence(
                name: "seq_import_job",
                schema: "Staging");

            migrationBuilder.DropSequence(
                name: "seq_site_code",
                schema: "Staging");

            migrationBuilder.DropSequence(
                name: "seq_ticket_number",
                schema: "Staging");

            migrationBuilder.DropSequence(
                name: "seq_vendor_code",
                schema: "Staging");
        }
    }
}

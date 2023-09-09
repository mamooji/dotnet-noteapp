using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "application_user",
                columns: table => new
                {
                    id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    first_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    last_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    avatar_url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    user_name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    normalized_user_name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    normalized_email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    email_confirmed = table.Column<bool>(type: "bit", nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    security_stamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    phone_number = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    phone_number_confirmed = table.Column<bool>(type: "bit", nullable: false),
                    two_factor_enabled = table.Column<bool>(type: "bit", nullable: false),
                    lockout_end = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    lockout_enabled = table.Column<bool>(type: "bit", nullable: false),
                    access_failed_count = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_application_user", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "device_flow_codes",
                columns: table => new
                {
                    user_code = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    device_code = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    subject_id = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    session_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    client_id = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    creation_time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    expiration = table.Column<DateTime>(type: "datetime2", nullable: false),
                    data = table.Column<string>(type: "nvarchar(max)", maxLength: 50000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_device_flow_codes", x => x.user_code);
                });

            migrationBuilder.CreateTable(
                name: "identity_role",
                columns: table => new
                {
                    id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    normalized_name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    concurrency_stamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_role", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "key",
                columns: table => new
                {
                    id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    version = table.Column<int>(type: "int", nullable: false),
                    created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    use = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    algorithm = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    is_x509_certificate = table.Column<bool>(type: "bit", nullable: false),
                    data_protected = table.Column<bool>(type: "bit", nullable: false),
                    data = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_key", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "persisted_grant",
                columns: table => new
                {
                    key = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    subject_id = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    session_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    client_id = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    creation_time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    expiration = table.Column<DateTime>(type: "datetime2", nullable: true),
                    consumed_time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    data = table.Column<string>(type: "nvarchar(max)", maxLength: 50000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_persisted_grant", x => x.key);
                });

            migrationBuilder.CreateTable(
                name: "identity_user_claim<string>",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    claim_type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    claim_value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_user_claim<string>", x => x.id);
                    table.ForeignKey(
                        name: "fk_identity_user_claim<string>_application_user_user_id",
                        column: x => x.user_id,
                        principalTable: "application_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "identity_user_login<string>",
                columns: table => new
                {
                    login_provider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    provider_key = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    provider_display_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    user_id = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_user_login<string>", x => new { x.login_provider, x.provider_key });
                    table.ForeignKey(
                        name: "fk_identity_user_login<string>_application_user_user_id",
                        column: x => x.user_id,
                        principalTable: "application_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "identity_user_token<string>",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    login_provider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_user_token<string>", x => new { x.user_id, x.login_provider, x.name });
                    table.ForeignKey(
                        name: "fk_identity_user_token<string>_application_user_user_id",
                        column: x => x.user_id,
                        principalTable: "application_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "note",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    application_user_id = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    last_modified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    last_modified_by = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_note", x => x.id);
                    table.ForeignKey(
                        name: "fk_note_application_user_application_user_id",
                        column: x => x.application_user_id,
                        principalTable: "application_user",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "identity_role_claim<string>",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    role_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    claim_type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    claim_value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_role_claim<string>", x => x.id);
                    table.ForeignKey(
                        name: "fk_identity_role_claim<string>_identity_role_role_id",
                        column: x => x.role_id,
                        principalTable: "identity_role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "identity_user_role<string>",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    role_id = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_user_role<string>", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "fk_identity_user_role<string>_application_user_user_id",
                        column: x => x.user_id,
                        principalTable: "application_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_identity_user_role<string>_identity_role_role_id",
                        column: x => x.role_id,
                        principalTable: "identity_role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "email_index",
                table: "application_user",
                column: "normalized_email");

            migrationBuilder.CreateIndex(
                name: "user_name_index",
                table: "application_user",
                column: "normalized_user_name",
                unique: true,
                filter: "[normalized_user_name] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_device_flow_codes_device_code",
                table: "device_flow_codes",
                column: "device_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_device_flow_codes_expiration",
                table: "device_flow_codes",
                column: "expiration");

            migrationBuilder.CreateIndex(
                name: "role_name_index",
                table: "identity_role",
                column: "normalized_name",
                unique: true,
                filter: "[normalized_name] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_identity_role_claim<string>_role_id",
                table: "identity_role_claim<string>",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_identity_user_claim<string>_user_id",
                table: "identity_user_claim<string>",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_identity_user_login<string>_user_id",
                table: "identity_user_login<string>",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_identity_user_role<string>_role_id",
                table: "identity_user_role<string>",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_key_use",
                table: "key",
                column: "use");

            migrationBuilder.CreateIndex(
                name: "ix_note_application_user_id",
                table: "note",
                column: "application_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_persisted_grant_consumed_time",
                table: "persisted_grant",
                column: "consumed_time");

            migrationBuilder.CreateIndex(
                name: "ix_persisted_grant_expiration",
                table: "persisted_grant",
                column: "expiration");

            migrationBuilder.CreateIndex(
                name: "ix_persisted_grant_subject_id_client_id_type",
                table: "persisted_grant",
                columns: new[] { "subject_id", "client_id", "type" });

            migrationBuilder.CreateIndex(
                name: "ix_persisted_grant_subject_id_session_id_type",
                table: "persisted_grant",
                columns: new[] { "subject_id", "session_id", "type" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "device_flow_codes");

            migrationBuilder.DropTable(
                name: "identity_role_claim<string>");

            migrationBuilder.DropTable(
                name: "identity_user_claim<string>");

            migrationBuilder.DropTable(
                name: "identity_user_login<string>");

            migrationBuilder.DropTable(
                name: "identity_user_role<string>");

            migrationBuilder.DropTable(
                name: "identity_user_token<string>");

            migrationBuilder.DropTable(
                name: "key");

            migrationBuilder.DropTable(
                name: "note");

            migrationBuilder.DropTable(
                name: "persisted_grant");

            migrationBuilder.DropTable(
                name: "identity_role");

            migrationBuilder.DropTable(
                name: "application_user");
        }
    }
}

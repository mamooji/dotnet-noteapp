using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class optionalApplicationUserIdToNote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_note_application_user_application_user_id",
                table: "note");

            migrationBuilder.AlterColumn<string>(
                name: "application_user_id",
                table: "note",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "fk_note_application_user_application_user_id",
                table: "note",
                column: "application_user_id",
                principalTable: "application_user",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_note_application_user_application_user_id",
                table: "note");

            migrationBuilder.AlterColumn<string>(
                name: "application_user_id",
                table: "note",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_note_application_user_application_user_id",
                table: "note",
                column: "application_user_id",
                principalTable: "application_user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class noteOneToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_note_application_user_application_user_id",
                table: "note");

            migrationBuilder.DropIndex(
                name: "ix_note_application_user_id",
                table: "note");

            migrationBuilder.AlterColumn<int>(
                name: "application_user_id",
                table: "note",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "application_user_id1",
                table: "note",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_note_application_user_id1",
                table: "note",
                column: "application_user_id1");

            migrationBuilder.AddForeignKey(
                name: "fk_note_application_user_application_user_id1",
                table: "note",
                column: "application_user_id1",
                principalTable: "application_user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_note_application_user_application_user_id1",
                table: "note");

            migrationBuilder.DropIndex(
                name: "ix_note_application_user_id1",
                table: "note");

            migrationBuilder.DropColumn(
                name: "application_user_id1",
                table: "note");

            migrationBuilder.AlterColumn<string>(
                name: "application_user_id",
                table: "note",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "ix_note_application_user_id",
                table: "note",
                column: "application_user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_note_application_user_application_user_id",
                table: "note",
                column: "application_user_id",
                principalTable: "application_user",
                principalColumn: "id");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class usuarioSede : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SedeId",
                table: "usuario",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuario_SedeId",
                table: "usuario",
                column: "SedeId");

            migrationBuilder.AddForeignKey(
                name: "FK_usuario_sede_SedeId",
                table: "usuario",
                column: "SedeId",
                principalTable: "sede",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_usuario_sede_SedeId",
                table: "usuario");

            migrationBuilder.DropIndex(
                name: "IX_usuario_SedeId",
                table: "usuario");

            migrationBuilder.DropColumn(
                name: "SedeId",
                table: "usuario");
        }
    }
}

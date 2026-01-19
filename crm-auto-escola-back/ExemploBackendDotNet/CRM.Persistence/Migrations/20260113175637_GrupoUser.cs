using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class GrupoUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UsuarioId",
                table: "grupowhatsapp",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_grupowhatsapp_UsuarioId",
                table: "grupowhatsapp",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_grupowhatsapp_usuario_UsuarioId",
                table: "grupowhatsapp",
                column: "UsuarioId",
                principalTable: "usuario",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_grupowhatsapp_usuario_UsuarioId",
                table: "grupowhatsapp");

            migrationBuilder.DropIndex(
                name: "IX_grupowhatsapp_UsuarioId",
                table: "grupowhatsapp");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "grupowhatsapp");
        }
    }
}

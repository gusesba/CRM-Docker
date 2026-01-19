using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class VendedorAtual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VendedorAtualId",
                table: "venda",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_venda_VendedorAtualId",
                table: "venda",
                column: "VendedorAtualId");

            migrationBuilder.AddForeignKey(
                name: "FK_venda_usuario_VendedorAtualId",
                table: "venda",
                column: "VendedorAtualId",
                principalTable: "usuario",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_venda_usuario_VendedorAtualId",
                table: "venda");

            migrationBuilder.DropIndex(
                name: "IX_venda_VendedorAtualId",
                table: "venda");

            migrationBuilder.DropColumn(
                name: "VendedorAtualId",
                table: "venda");
        }
    }
}

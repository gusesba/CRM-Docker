using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CRM.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class venda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "venda",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SedeId = table.Column<int>(type: "integer", nullable: false),
                    DataInicial = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VendedorId = table.Column<int>(type: "integer", nullable: false),
                    DataAlteracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Cliente = table.Column<string>(type: "text", nullable: false),
                    Genero = table.Column<int>(type: "integer", nullable: false),
                    Origem = table.Column<int>(type: "integer", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Fone = table.Column<string>(type: "text", nullable: false),
                    Celular = table.Column<string>(type: "text", nullable: false),
                    Contato = table.Column<string>(type: "text", nullable: false),
                    ComoConheceu = table.Column<string>(type: "text", nullable: false),
                    MotivoEscolha = table.Column<string>(type: "text", nullable: false),
                    ServicoId = table.Column<int>(type: "integer", nullable: false),
                    Obs = table.Column<string>(type: "text", nullable: false),
                    CondicaoVendaId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ValorVenda = table.Column<decimal>(type: "numeric", nullable: false),
                    Indicacao = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_venda", x => x.Id);
                    table.ForeignKey(
                        name: "FK_venda_condicaoVenda_CondicaoVendaId",
                        column: x => x.CondicaoVendaId,
                        principalTable: "condicaoVenda",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_venda_sede_SedeId",
                        column: x => x.SedeId,
                        principalTable: "sede",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_venda_servico_ServicoId",
                        column: x => x.ServicoId,
                        principalTable: "servico",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_venda_usuario_VendedorId",
                        column: x => x.VendedorId,
                        principalTable: "usuario",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_venda_CondicaoVendaId",
                table: "venda",
                column: "CondicaoVendaId");

            migrationBuilder.CreateIndex(
                name: "IX_venda_SedeId",
                table: "venda",
                column: "SedeId");

            migrationBuilder.CreateIndex(
                name: "IX_venda_ServicoId",
                table: "venda",
                column: "ServicoId");

            migrationBuilder.CreateIndex(
                name: "IX_venda_VendedorId",
                table: "venda",
                column: "VendedorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "venda");
        }
    }
}

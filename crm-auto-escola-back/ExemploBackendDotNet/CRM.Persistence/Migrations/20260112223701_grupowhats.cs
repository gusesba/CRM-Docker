using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CRM.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class grupowhats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "grupowhatsapp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grupowhatsapp", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "grupovendawhatsapp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdVendaWhats = table.Column<int>(type: "integer", nullable: false),
                    IdGrupo = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grupovendawhatsapp", x => x.Id);
                    table.ForeignKey(
                        name: "FK_grupovendawhatsapp_grupowhatsapp_IdGrupo",
                        column: x => x.IdGrupo,
                        principalTable: "grupowhatsapp",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_grupovendawhatsapp_vendawhatsapp_IdVendaWhats",
                        column: x => x.IdVendaWhats,
                        principalTable: "vendawhatsapp",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_grupovendawhatsapp_IdGrupo",
                table: "grupovendawhatsapp",
                column: "IdGrupo");

            migrationBuilder.CreateIndex(
                name: "IX_grupovendawhatsapp_IdVendaWhats",
                table: "grupovendawhatsapp",
                column: "IdVendaWhats");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "grupovendawhatsapp");

            migrationBuilder.DropTable(
                name: "grupowhatsapp");
        }
    }
}

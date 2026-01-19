using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CRM.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Whats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "vendawhatsapp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VendaId = table.Column<int>(type: "integer", nullable: false),
                    WhatsappChatId = table.Column<string>(type: "text", nullable: false),
                    WhatsappUserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vendawhatsapp", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vendawhatsapp_venda_VendaId",
                        column: x => x.VendaId,
                        principalTable: "venda",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_vendawhatsapp_VendaId",
                table: "vendawhatsapp",
                column: "VendaId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "vendawhatsapp");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CRM.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class backupWhats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "chatwhatsapp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    WhatsappChatId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chatwhatsapp", x => x.Id);
                    table.ForeignKey(
                        name: "FK_chatwhatsapp_usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuario",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "mensagemwhatsapp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChatWhatsappId = table.Column<int>(type: "integer", nullable: false),
                    MensagemId = table.Column<string>(type: "text", nullable: false),
                    Body = table.Column<string>(type: "text", nullable: true),
                    FromMe = table.Column<bool>(type: "boolean", nullable: false),
                    Timestamp = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    HasMedia = table.Column<bool>(type: "boolean", nullable: false),
                    MediaUrl = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mensagemwhatsapp", x => x.Id);
                    table.ForeignKey(
                        name: "FK_mensagemwhatsapp_chatwhatsapp_ChatWhatsappId",
                        column: x => x.ChatWhatsappId,
                        principalTable: "chatwhatsapp",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_chatwhatsapp_UsuarioId_WhatsappChatId",
                table: "chatwhatsapp",
                columns: new[] { "UsuarioId", "WhatsappChatId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_mensagemwhatsapp_ChatWhatsappId_MensagemId",
                table: "mensagemwhatsapp",
                columns: new[] { "ChatWhatsappId", "MensagemId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mensagemwhatsapp");

            migrationBuilder.DropTable(
                name: "chatwhatsapp");
        }
    }
}

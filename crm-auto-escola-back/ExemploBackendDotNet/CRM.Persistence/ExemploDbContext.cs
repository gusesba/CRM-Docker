using Exemplo.Domain.Model;
using Exemplo.Domain.Model.Enum;
using Microsoft.EntityFrameworkCore;

namespace Exemplo.Persistence
{
    public class ExemploDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public ExemploDbContext(DbContextOptions<ExemploDbContext> options) : base(options) { }
        
        public DbSet<ExemploModel> Exemplo { get; set; }

        public DbSet<UsuarioModel> Usuario { get; set; }

        public DbSet<ServicoModel> Servico { get; set; }

        public DbSet<SedeModel> Sede { get; set; }

        public DbSet<CondicaoVendaModel> CondicaoVenda { get; set; }

        public DbSet<VendaModel> Venda { get; set; }

        public DbSet<AgendamentoModel> Agendamento { get; set; }
        public DbSet<VendaWhatsappModel> VendaWhatsapp { get; set; }
        public DbSet<GrupoWhatsappModel> GrupoWhatsapp { get; set; }
        public DbSet<GrupoVendaWhatsappModel> GrupoVendaWhatsapp { get; set; }
        public DbSet<ChatWhatsappModel> ChatWhatsapp { get; set; }
        public DbSet<MensagemWhatsappModel> MensagemWhatsapp { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ExemploModel>(entity =>
            {
                entity.ToTable("exemplo");
                entity.HasKey(p => p.Campo1);
                entity.Property(p => p.Campo1).ValueGeneratedOnAdd();
                entity.Property(p => p.Campo2).HasMaxLength(500).IsRequired();
                entity.Property(p => p.Campo3);
                entity.HasIndex(p => p.Campo1);
            });

            modelBuilder.Entity<UsuarioModel>(entity =>
            {
                entity.ToTable("usuario");
                entity.HasKey(p => p.Id);
                entity.Property(p=>p.Id).ValueGeneratedOnAdd();
                entity.Property(p=>p.Usuario).HasMaxLength(200).IsRequired();
                entity.Property(p=>p.SenhaHash).HasMaxLength(256).IsRequired();
                entity.Property(p => p.Nome).HasMaxLength(200).IsRequired();
                entity.Property(p => p.IsAdmin).IsRequired(true).HasDefaultValue(false);
                entity.Property(p => p.Status).HasDefaultValue(StatusUsuarioEnum.Ativo);
                entity.Property(p => p.SedeId);
                entity.HasIndex(p => p.Usuario).IsUnique();

                entity.HasMany(p => p.Vendas)
                        .WithOne(p => p.Vendedor)
                        .HasForeignKey(p => p.VendedorId)
                        .OnDelete(DeleteBehavior.NoAction);

                entity.HasMany(p => p.VendasAtuais)
                      .WithOne(p => p.VendedorAtual)
                      .HasForeignKey(p => p.VendedorAtualId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(p => p.Sede)
                      .WithMany(p => p.Usuarios)
                      .HasForeignKey(p => p.SedeId)
                      .OnDelete(DeleteBehavior.NoAction);
            });



            modelBuilder.Entity<ServicoModel>(entity =>
            {
                entity.ToTable("servico");
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Id).ValueGeneratedOnAdd();
                entity.Property(p => p.Nome).HasMaxLength(200).IsRequired();

                entity.HasMany(p => p.Vendas)
                        .WithOne(p => p.Servico)
                        .HasForeignKey(p => p.ServicoId)
                        .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<SedeModel>(entity =>
            {
                entity.ToTable("sede");
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Id).ValueGeneratedOnAdd();
                entity.Property(p => p.Nome).HasMaxLength(200).IsRequired();
                entity.Property(p => p.DataInclusao).IsRequired();
                entity.Property(p => p.Ativo).HasDefaultValue(true);

                entity.HasMany(p => p.Vendas)
                        .WithOne(p => p.Sede)
                        .HasForeignKey(p => p.SedeId)
                        .OnDelete(DeleteBehavior.NoAction);

                entity.HasMany(p => p.Usuarios)
                        .WithOne(p => p.Sede)
                        .HasForeignKey(p => p.SedeId)
                        .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<CondicaoVendaModel>(entity =>
            {
                entity.ToTable("condicaoVenda");
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Id).ValueGeneratedOnAdd();
                entity.Property(p => p.Nome).HasMaxLength(200).IsRequired();

                entity.HasMany(p => p.Vendas)
                        .WithOne(p => p.CondicaoVenda)
                        .HasForeignKey(p => p.CondicaoVendaId)
                        .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<VendaModel>(entity =>
            {
                entity.ToTable("venda");
                entity.HasKey(entity => entity.Id);
                entity.Property(p => p.Id).IsRequired();
                entity.Property(p => p.SedeId);
                entity.Property(p => p.DataInicial).IsRequired();
                entity.Property(p => p.VendedorId).IsRequired();
                entity.Property(p => p.DataAlteracao);
                entity.Property(p => p.Cliente).IsRequired();
                entity.Property(p => p.Genero);
                entity.Property(p => p.Origem);
                entity.Property(p => p.Email);
                entity.Property(p => p.Fone);
                entity.Property(p => p.Contato);
                entity.Property(p => p.ComoConheceu);
                entity.Property(p => p.MotivoEscolha);
                entity.Property(p => p.ServicoId);
                entity.Property(p => p.Obs);
                entity.Property(p => p.CondicaoVendaId);
                entity.Property(p => p.Status).IsRequired();
                entity.Property(p => p.ValorVenda);
                entity.Property(p => p.Indicacao);
                entity.Property(p => p.Contrato);
                entity.Property(p => p.DataNascimento);
                entity.Property(p => p.VendedorAtualId);

                entity.HasOne(p => p.VendedorAtual)
                      .WithMany(p => p.VendasAtuais)
                      .HasForeignKey(p => p.VendedorAtualId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(p => p.Sede)
                      .WithMany(p => p.Vendas)
                      .HasForeignKey(p => p.SedeId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(p => p.Vendedor)
                      .WithMany(p => p.Vendas)
                      .HasForeignKey(p => p.VendedorId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(p => p.Servico)
                      .WithMany(p => p.Vendas)
                      .HasForeignKey(p => p.ServicoId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(p => p.CondicaoVenda)
                      .WithMany(p => p.Vendas)
                      .HasForeignKey(p => p.CondicaoVendaId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasMany(p => p.Agendamentos)
                        .WithOne(p => p.Venda)
                        .HasForeignKey(p => p.VendaId)
                        .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(v => v.VendaWhatsapp)
                  .WithOne(vw => vw.Venda)
                  .HasForeignKey<VendaWhatsappModel>(vw => vw.VendaId)
                  .OnDelete(DeleteBehavior.NoAction);


            });

            modelBuilder.Entity<AgendamentoModel>(entity =>
            {
                entity.ToTable("agendamento");
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Id).ValueGeneratedOnAdd();
                entity.Property(p => p.VendaId).IsRequired();
                entity.Property(p => p.DataAgendamento).IsRequired();
                entity.Property(p => p.Obs);

                entity.HasOne(p => p.Venda)
                      .WithMany(p => p.Agendamentos)
                      .HasForeignKey(p => p.VendaId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<VendaWhatsappModel>(entity =>
            {
                entity.ToTable("vendawhatsapp");
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Id).ValueGeneratedOnAdd();
                entity.Property(p => p.VendaId).IsRequired();
                entity.Property(p => p.WhatsappUserId).IsRequired();
                entity.Property(p => p.WhatsappChatId).IsRequired();

                entity.HasOne(p => p.Venda)
                  .WithOne(v => v.VendaWhatsapp)
                  .HasForeignKey<VendaWhatsappModel>(p => p.VendaId)
                  .OnDelete(DeleteBehavior.NoAction);

                entity.HasMany(entity => entity.GruposVendaWhatsapp)
                      .WithOne(gv => gv.VendaWhatsapp)
                      .HasForeignKey(gv => gv.IdVendaWhats)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<GrupoWhatsappModel>(entity =>
            {
                entity.ToTable("grupowhatsapp");
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Id).ValueGeneratedOnAdd();
                entity.Property(p => p.Nome).IsRequired();
                entity.Property(p => p.UsuarioId).IsRequired();

                entity.HasOne(p => p.Usuario)
                      .WithMany(p => p.GruposWhatsapp)
                      .HasForeignKey(p => p.UsuarioId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasMany(entity => entity.GruposVendaWhatsapp)
                      .WithOne(gv => gv.GrupoWhatsapp)
                      .HasForeignKey(gv => gv.IdGrupo)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<GrupoVendaWhatsappModel>(entity =>
            {
                entity.ToTable("grupovendawhatsapp");
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Id).ValueGeneratedOnAdd();
                entity.Property(p => p.IdVendaWhats).IsRequired();
                entity.Property(p => p.IdGrupo).IsRequired();

                entity.HasOne(p => p.VendaWhatsapp)
                      .WithMany(vw => vw.GruposVendaWhatsapp)
                      .HasForeignKey(p => p.IdVendaWhats)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(p => p.GrupoWhatsapp)
                      .WithMany(g => g.GruposVendaWhatsapp)
                      .HasForeignKey(p => p.IdGrupo)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<ChatWhatsappModel>(entity =>
            {
                entity.ToTable("chatwhatsapp");
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Id).ValueGeneratedOnAdd();
                entity.Property(p => p.UsuarioId).IsRequired();
                entity.Property(p => p.WhatsappChatId).IsRequired();
                entity.Property(p => p.NomeChat);
                entity.HasIndex(p => new { p.UsuarioId, p.WhatsappChatId }).IsUnique();

                entity.HasOne(p => p.Usuario)
                      .WithMany(u => u.ChatsWhatsapp)
                      .HasForeignKey(p => p.UsuarioId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasMany(p => p.Mensagens)
                      .WithOne(m => m.ChatWhatsapp)
                      .HasForeignKey(m => m.ChatWhatsappId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<MensagemWhatsappModel>(entity =>
            {
                entity.ToTable("mensagemwhatsapp");
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Id).ValueGeneratedOnAdd();
                entity.Property(p => p.ChatWhatsappId).IsRequired();
                entity.Property(p => p.MensagemId).IsRequired();
                entity.Property(p => p.Body);
                entity.Property(p => p.FromMe).IsRequired();
                entity.Property(p => p.Timestamp).IsRequired();
                entity.Property(p => p.Type).IsRequired();
                entity.Property(p => p.HasMedia).IsRequired();
                entity.Property(p => p.MediaUrl);
                entity.HasIndex(p => new { p.ChatWhatsappId, p.MensagemId }).IsUnique();
            });
        }
    }
}

using CondominioReservas.API.Models;
using CondominioReservas.Models;
using Microsoft.EntityFrameworkCore;

namespace CondominioReservas.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<TipoEspaco> TiposEspaco { get; set; }
        public DbSet<Espaco> Espacos { get; set; }
        public DbSet<ItemAdicional> ItensAdicionais { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<ItemReserva> ItensReserva { get; set; }
        public DbSet<Pagamento> Pagamentos { get; set; }
        public DbSet<RegraDisponibilidade> RegrasDisponibilidade { get; set; }
        public DbSet<LogAtividade> LogsAtividade { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurações de relacionamentos
            modelBuilder.Entity<Espaco>()
                .HasOne(e => e.TipoEspaco)
                .WithMany(t => t.Espacos)
                .HasForeignKey(e => e.TipoEspacoId);

            modelBuilder.Entity<Reserva>()
                .HasOne(r => r.Usuario)
                .WithMany(u => u.Reservas)
                .HasForeignKey(r => r.UsuarioId);

            modelBuilder.Entity<Reserva>()
                .HasOne(r => r.Espaco)
                .WithMany(e => e.Reservas)
                .HasForeignKey(r => r.EspacoId);

            modelBuilder.Entity<ItemReserva>()
                .HasOne(ir => ir.Reserva)
                .WithMany(r => r.ItensReserva)
                .HasForeignKey(ir => ir.ReservaId);

            modelBuilder.Entity<ItemReserva>()
                .HasOne(ir => ir.ItemAdicional)
                .WithMany()
                .HasForeignKey(ir => ir.ItemId);

            modelBuilder.Entity<Pagamento>()
                .HasOne(p => p.Reserva)
                .WithMany()
                .HasForeignKey(p => p.ReservaId);

            // Dados iniciais
            modelBuilder.Entity<TipoEspaco>().HasData(
                new TipoEspaco { Id = 1, Nome = "Salão de Festas", RequerePagemento = true, ValorReserva = 500.00m, DuracaoMaximaHoras = 24, DuracaoMinimaHoras = 24 },
                new TipoEspaco { Id = 2, Nome = "Quadra Poliesportiva", RequerePagemento = false, ValorReserva = 0, DuracaoMaximaHoras = 4, DuracaoMinimaHoras = 1 },
                new TipoEspaco { Id = 3, Nome = "Campo de Futebol", RequerePagemento = false, ValorReserva = 0, DuracaoMaximaHoras = 4, DuracaoMinimaHoras = 1 },
                new TipoEspaco { Id = 4, Nome = "Quadra de Areia", RequerePagemento = false, ValorReserva = 0, DuracaoMaximaHoras = 4, DuracaoMinimaHoras = 1 },
                new TipoEspaco { Id = 5, Nome = "Quiosque", RequerePagemento = false, ValorReserva = 0, DuracaoMaximaHoras = 4, DuracaoMinimaHoras = 1 }
            );

            modelBuilder.Entity<Espaco>().HasData(
                new Espaco { Id = 1, Nome = "Salão de Festas Principal", TipoEspacoId = 1, Capacidade = 100, Ativo = true },
                new Espaco { Id = 2, Nome = "Quadra Poliesportiva", TipoEspacoId = 2, Capacidade = 20, Ativo = true },
                new Espaco { Id = 3, Nome = "Campo de Futebol", TipoEspacoId = 3, Capacidade = 22, Ativo = true },
                new Espaco { Id = 4, Nome = "Quadra de Areia", TipoEspacoId = 4, Capacidade = 12, Ativo = true },
                new Espaco { Id = 5, Nome = "Quiosque 1", TipoEspacoId = 5, Capacidade = 15, Ativo = true },
                new Espaco { Id = 6, Nome = "Quiosque 2", TipoEspacoId = 5, Capacidade = 15, Ativo = true }
            );

            modelBuilder.Entity<ItemAdicional>().HasData(
                new ItemAdicional { Id = 1, Nome = "Cadeira", QuantidadeDisponivel = 50, TipoEspacoId = 5 },
                new ItemAdicional { Id = 2, Nome = "Mesa", QuantidadeDisponivel = 10, TipoEspacoId = 5 },
                new ItemAdicional { Id = 3, Nome = "Churrasqueira", QuantidadeDisponivel = 2, TipoEspacoId = 5 }
            );
        }
    }
}

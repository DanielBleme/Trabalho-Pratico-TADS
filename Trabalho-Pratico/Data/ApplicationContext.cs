using Microsoft.EntityFrameworkCore;
using Trabalho_Pratico.Models;

namespace Trabalho_Pratico.Data
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
        }
        public ApplicationContext()
        {
        }

        public DbSet<Fabricante> Fabricantes => Set<Fabricante>();
        public DbSet<Veiculo> Veiculos => Set<Veiculo>();
        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<Aluguel> Alugueis => Set<Aluguel>();
        public DbSet<Pagamento> Pagamentos => Set<Pagamento>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==========================================
            // 1. MAPEAMENTO: FABRICANTE
            // ==========================================
            modelBuilder.Entity<Fabricante>(entity =>
            {
                entity.ToTable("Fabricantes");

                entity.HasKey(f => f.id);

                entity.Property(f => f.nome)
                      .IsRequired();
                      
                entity.Property(f => f.CNPJ)
                       .IsRequired();
            });

            // ==========================================
            // 2. MAPEAMENTO: VEÍCULO
            // ==========================================
            modelBuilder.Entity<Veiculo>(entity =>
            {
                entity.ToTable("Veiculos");

                entity.HasKey(v => v.Id);

                entity.Property(v => v.Modelo)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(v => v.AnoFabricacao)
                      .IsRequired();

                entity.Property(v => v.Placa)
                      .IsRequired()
                      .HasMaxLength(10);

                // Regra: Placa única
                entity.HasIndex(v => v.Placa)
                      .IsUnique();

                entity.Property(v => v.Quilometragem)
                      .IsRequired();

                // Relacionamento 1:N -> Fabricante possui muitos Veículos
                entity.HasOne(v => v.Fabricante)
                      .WithMany(f => f.Veiculos)
                      .HasForeignKey(v => v.FabricanteId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================================
            // 3. MAPEAMENTO: CLIENTE
            // ==========================================
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.ToTable("Clientes");

                entity.HasKey(c => c.Id);

                entity.Property(c => c.Nome)
                      .IsRequired()
                      .HasMaxLength(150);

                entity.Property(c => c.CPF)
                      .IsRequired()
                      .HasMaxLength(14);

                entity.HasIndex(c => c.CPF)
                      .IsUnique();

                entity.Property(c => c.Email)
                      .IsRequired()
                      .HasMaxLength(150);

                entity.Property(c => c.Telefone)
                      .HasMaxLength(20);
            });

            // ==========================================
            // 4. MAPEAMENTO: ALUGUEL
            // ==========================================
            modelBuilder.Entity<Aluguel>(entity =>
            {
                entity.ToTable("Alugueis");

                entity.HasKey(a => a.Id);

                entity.Property(a => a.DataInicio)
                      .IsRequired();

                entity.Property(a => a.DataFimPrevista)
                      .IsRequired();

                entity.Property(a => a.DataDevolucao)
                      .IsRequired(false);

                entity.Property(a => a.QuilometragemInicial)
                      .IsRequired();

                entity.Property(a => a.QuilometragemFinal)
                      .IsRequired(false);

               
                entity.Property(a => a.ValorDiaria)
                      .HasColumnType("decimal(10,2)")
                      .IsRequired();

                entity.Property(a => a.ValorTotal)
                      .HasColumnType("decimal(10,2)")
                      .IsRequired(false);

                entity.HasOne(a => a.Cliente)
                      .WithMany(c => c.Alugueis)
                      .HasForeignKey(a => a.ClienteId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Veiculo)
                      .WithMany(v => v.Alugueis)
                      .HasForeignKey(a => a.VeiculoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================================
            // 5. MAPEAMENTO: PAGAMENTO 
            // ==========================================
            modelBuilder.Entity<Pagamento>(entity =>
            {
                entity.ToTable("Pagamentos");

                entity.HasKey(p => p.Id);

                entity.Property(p => p.DataPagamento)
                      .IsRequired();

                entity.Property(p => p.ValorPago)
                      .HasColumnType("decimal(10,2)")
                      .IsRequired();

                entity.Property(p => p.MetodoPagamento)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(p => p.Status)
                      .IsRequired()
                      .HasMaxLength(30);

                entity.HasOne(p => p.Aluguel)
                      .WithMany(a => a.Pagamentos)
                      .HasForeignKey(p => p.AluguelId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
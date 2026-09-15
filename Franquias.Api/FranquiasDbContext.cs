using Microsoft.EntityFrameworkCore;

namespace Franquias.Api
{
    public class FranquiasDbContext : DbContext
    {
        public FranquiasDbContext(DbContextOptions<FranquiasDbContext> options)
            : base(options)
        {
        }

        public DbSet<Franqueadora> Franqueadoras { get; set; }
        public DbSet<Unidade> Unidades { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Fornecedor> Fornecedores { get; set; }
        public DbSet<Produto> Produtos { get; set; }
        public DbSet<EstoqueProduto> Estoques { get; set; }
        public DbSet<MovimentoEstoque> MovimentosEstoque { get; set; }
        public DbSet<Venda> Vendas { get; set; }
        public DbSet<ItemVenda> ItensVenda { get; set; }
        public DbSet<Chamado> Chamados { get; set; }
        public DbSet<RoyaltyConfig> RoyaltyConfigs { get; set; }
        public DbSet<Responsavel> Responsaveis { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Franqueadora>()
                .HasIndex(f => f.Cnpj)
                .IsUnique();

            modelBuilder.Entity<Unidade>()
                .HasIndex(u => u.Cnpj)
                .IsUnique();

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Fornecedor>()
                .HasIndex(f => f.Cnpj)
                .IsUnique();

            modelBuilder.Entity<EstoqueProduto>()
                .HasIndex(e => new { e.ProdutoId, e.UnidadeId })
                .IsUnique();

            modelBuilder.Entity<Responsavel>()
                .HasIndex(r => new { r.UnidadeId, r.Cpf })
                .IsUnique();

            modelBuilder.Entity<Franqueadora>()
                .HasMany(f => f.Unidades)
                .WithOne(u => u.Franqueadora)
                .HasForeignKey(u => u.FranqueadoraId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MovimentoEstoque>()
                .HasOne(m => m.EstoqueProduto)
                .WithMany()
                .HasForeignKey(m => m.EstoqueUnidadeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Responsavel>()
                .HasOne(r => r.Unidade)
                .WithMany()
                .HasForeignKey(r => r.UnidadeId)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Franquias.Api
{
    public enum PerfilUsuario
    {
        Administrador,
        Gestor,
        Operador
    }

    public enum SituacaoProduto
    {
        Ativo,
        Inativo
    }

    public enum PrioridadeChamado
    {
        Baixa,
        Media,
        Alta,
        Critica
    }

    public enum StatusChamado
    {
        Aberto,
        EmAnalise,
        EmAndamento,
        Resolvido,
        Fechado
    }

    public enum TipoMovimentoEstoque
    {
        Entrada,
        Saida
    }

    [Table("Franqueadora")]
    public class Franqueadora
    {
        public int Id { get; set; }

        [Column("NomeFantasia")]
        public string NomeFantasia { get; set; } = string.Empty;

        [NotMapped]
        public string Nome
        {
            get => NomeFantasia;
            set => NomeFantasia = value;
        }

        [Column("RazaoSocial")]
        public string RazaoSocial { get; set; } = string.Empty;

        [Column("CNPJ")]
        public string Cnpj { get; set; } = string.Empty;

        [Column("Email")]
        public string Email { get; set; } = string.Empty;

        [Column("Ativa")]
        public bool Ativa { get; set; } = true;

        [Column("DataCadastro")]
        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

        public ICollection<Unidade> Unidades { get; set; } = new List<Unidade>();
    }

    [Table("UnidadeFranqueada")]
    public class Unidade
    {
        public int Id { get; set; }

        [Column("NomeUnidade")]
        public string Nome { get; set; } = string.Empty;

        [Column("CNPJ")]
        public string Cnpj { get; set; } = string.Empty;

        public string Endereco { get; set; } = string.Empty;

        public string Cidade { get; set; } = string.Empty;

        public string Estado { get; set; } = string.Empty;

        public string Telefone { get; set; } = string.Empty;

        [Column("PercentualRoyalty")]
        public decimal PercentualRoyalty { get; set; }

        public bool Ativa { get; set; } = true;

        public DateTime? DataInicio { get; set; }

        public int FranqueadoraId { get; set; }

        [ForeignKey(nameof(FranqueadoraId))]
        public Franqueadora? Franqueadora { get; set; }
    }

    [Table("usuarios")]
    public class Usuario
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string SenhaHash { get; set; } = string.Empty;

        [Column("perfilId")]
        public PerfilUsuario Perfil { get; set; } = PerfilUsuario.Operador;

        [Column("unidadeId")]
        public int? UnidadeId { get; set; }

        [ForeignKey(nameof(UnidadeId))]
        public Unidade? Unidade { get; set; }

        [Column("ativo")]
        public bool Ativo { get; set; } = true;
    }

    [Table("Categoria")]
    public class Categoria
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public bool Ativa { get; set; } = true;
    }

    [Table("Fornecedor")]
    public class Fornecedor
    {
        public int Id { get; set; }

        [Column("RazaoSocial")]
        public string Nome { get; set; } = string.Empty;

        [Column("CNPJ")]
        public string Cnpj { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Telefone { get; set; } = string.Empty;

        public bool Ativo { get; set; } = true;
    }

    [Table("Produto")]
    public class Produto
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string Descricao { get; set; } = string.Empty;

        [Column("PrecoBase")]
        public decimal Preco { get; set; }

        [Column("EstoqueMinimo")]
        public int EstoqueMinimo { get; set; }

        public int CategoriaId { get; set; }

        public int? FornecedorId { get; set; }

        [NotMapped]
        public SituacaoProduto Situacao
        {
            get => Ativo ? SituacaoProduto.Ativo : SituacaoProduto.Inativo;
            set => Ativo = value == SituacaoProduto.Ativo;
        }

        [Column("Ativo")]
        public bool Ativo { get; set; } = true;

        [ForeignKey(nameof(CategoriaId))]
        public Categoria? Categoria { get; set; }

        [ForeignKey(nameof(FornecedorId))]
        public Fornecedor? Fornecedor { get; set; }
    }

    [Table("EstoqueUnidade")]
    public class EstoqueProduto
    {
        public int Id { get; set; }

        public int ProdutoId { get; set; }

        public int UnidadeId { get; set; }

        [Column("QuantidadeSaldo")]
        public int Quantidade { get; set; }

        [ForeignKey(nameof(ProdutoId))]
        public Produto? Produto { get; set; }

        [ForeignKey(nameof(UnidadeId))]
        public Unidade? Unidade { get; set; }
    }

    [Table("MovimentacaoEstoque")]
    public class MovimentoEstoque
    {
        public int Id { get; set; }

        [Column("EstoqueUnidadeId")]
        public int EstoqueUnidadeId { get; set; }

        public TipoMovimentoEstoque Tipo { get; set; }

        public int Quantidade { get; set; }

        public string Motivo { get; set; } = string.Empty;

        [Column("DataMovimentacao")]
        public DateTime DataMovimento { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(EstoqueUnidadeId))]
        public EstoqueProduto? EstoqueProduto { get; set; }
    }

    [Table("Venda")]
    public class Venda
    {
        public int Id { get; set; }

        public int UnidadeId { get; set; }

        public int UsuarioId { get; set; }

        public DateTime DataVenda { get; set; } = DateTime.UtcNow;

        [Column("ValorTotal")]
        public decimal Total { get; set; }

        public string Status { get; set; } = "Concluida";

        [ForeignKey(nameof(UnidadeId))]
        public Unidade? Unidade { get; set; }

        [ForeignKey(nameof(UsuarioId))]
        public Usuario? Usuario { get; set; }

        public ICollection<ItemVenda> Itens { get; set; } = new List<ItemVenda>();
    }

    [Table("ItemVenda")]
    public class ItemVenda
    {
        public int Id { get; set; }

        public int VendaId { get; set; }

        public int ProdutoId { get; set; }

        public int Quantidade { get; set; }

        public decimal PrecoUnitario { get; set; }

        public decimal Subtotal { get; set; }

        [ForeignKey(nameof(VendaId))]
        public Venda? Venda { get; set; }

        [ForeignKey(nameof(ProdutoId))]
        public Produto? Produto { get; set; }
    }

    [Table("ChamadoSuporte")]
    public class Chamado
    {
        public int Id { get; set; }

        public string Titulo { get; set; } = string.Empty;

        public string Descricao { get; set; } = string.Empty;

        public string Categoria { get; set; } = string.Empty;

        public PrioridadeChamado Prioridade { get; set; } = PrioridadeChamado.Media;

        public StatusChamado Status { get; set; } = StatusChamado.Aberto;

        public int UnidadeId { get; set; }

        [Column("UsuarioAberturaId")]
        public int UsuarioId { get; set; }

        public DateTime DataAbertura { get; set; } = DateTime.UtcNow;

        [Column("DataEncerramento")]
        public DateTime? DataFechamento { get; set; }

        [ForeignKey(nameof(UnidadeId))]
        public Unidade? Unidade { get; set; }

        [ForeignKey(nameof(UsuarioId))]
        public Usuario? Usuario { get; set; }
    }

    [Table("CobrancaRoyalty")]
    public class RoyaltyConfig
    {
        public int Id { get; set; }

        public int UnidadeId { get; set; }

        public int MesReferencia { get; set; }

        public int AnoReferencia { get; set; }

        [Column("FaturamentoTotalPeriodo")]
        public decimal FaturamentoTotalPeriodo { get; set; }

        [Column("PercentualAplicado")]
        public decimal Percentual { get; set; }

        [Column("ValorDevido")]
        public decimal ValorDevido { get; set; }

        [Column("StatusPagamento")]
        public string StatusPagamento { get; set; } = "Pendente";

        [ForeignKey(nameof(UnidadeId))]
        public Unidade? Unidade { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace Franquias.Api.DTOs
{
    public class CriarProdutoDto
    {
        [Required]
        public string Nome { get; set; } = string.Empty;

        public string Descricao { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue)]
        public decimal Preco { get; set; }

        public int EstoqueMinimo { get; set; }

        [Required]
        public int CategoriaId { get; set; }

        public int? FornecedorId { get; set; }

        public bool Ativo { get; set; } = true;
    }

    public class AtualizarProdutoDto
    {
        public string? Nome { get; set; }
        public string? Descricao { get; set; }
        public decimal? Preco { get; set; }
        public int? EstoqueMinimo { get; set; }
        public int? CategoriaId { get; set; }
        public int? FornecedorId { get; set; }
        public bool? Ativo { get; set; }
    }

    public class ProdutoDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public int EstoqueMinimo { get; set; }
        public int CategoriaId { get; set; }
        public int? FornecedorId { get; set; }
        public bool Ativo { get; set; }
    }
}

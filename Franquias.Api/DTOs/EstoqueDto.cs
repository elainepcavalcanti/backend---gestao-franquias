using System.ComponentModel.DataAnnotations;

namespace Franquias.Api.DTOs
{
    public class MovimentoEstoqueDto
    {
        [Required]
        public int ProdutoId { get; set; }

        [Required]
        public int UnidadeId { get; set; }

        [Required]
        public TipoMovimentoEstoque Tipo { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantidade { get; set; }

        [Required]
        public string Motivo { get; set; } = string.Empty;
    }

    public class EstoqueResumoDto
    {
        public int ProdutoId { get; set; }
        public string ProdutoNome { get; set; } = string.Empty;
        public int UnidadeId { get; set; }
        public string UnidadeNome { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public int EstoqueMinimo { get; set; }
        public bool EstoqueCritico => Quantidade <= EstoqueMinimo;
    }
}

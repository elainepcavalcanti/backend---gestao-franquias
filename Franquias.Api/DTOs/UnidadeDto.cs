using System;
using System.ComponentModel.DataAnnotations;

namespace Franquias.Api.DTOs
{
    public class CriarUnidadeDto
    {
        [Required]
        public string Nome { get; set; } = string.Empty;

        [Required]
        public string Cnpj { get; set; } = string.Empty;

        [Required]
        public string Endereco { get; set; } = string.Empty;

        public string Cidade { get; set; } = string.Empty;

        public string Estado { get; set; } = string.Empty;

        public string Telefone { get; set; } = string.Empty;

        [Range(0, 100)]
        public decimal PercentualRoyalty { get; set; }

        [Required]
        public int FranqueadoraId { get; set; }
    }

    public class AtualizarUnidadeDto
    {
        public string? Nome { get; set; }
        public string? Cnpj { get; set; }
        public string? Endereco { get; set; }
        public string? Cidade { get; set; }
        public string? Estado { get; set; }
        public string? Telefone { get; set; }
        public decimal? PercentualRoyalty { get; set; }
        public bool? Ativa { get; set; }
        public DateTime? DataInicio { get; set; }
    }

    public class UnidadeDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Cnpj { get; set; } = string.Empty;
        public string Endereco { get; set; } = string.Empty;
        public string Cidade { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public decimal PercentualRoyalty { get; set; }
        public bool Ativa { get; set; }
        public DateTime? DataInicio { get; set; }
        public int FranqueadoraId { get; set; }
    }
}

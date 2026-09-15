using System.ComponentModel.DataAnnotations;

namespace Franquias.Api.DTOs
{
    public class CriarResponsavelDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int UnidadeId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [StringLength(14, MinimumLength = 11)]
        public string Cpf { get; set; } = string.Empty;

        [StringLength(20)]
        public string Telefone { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;
    }

    public class AtualizarResponsavelDto
    {
        [StringLength(100, MinimumLength = 3)]
        public string? Nome { get; set; }

        [StringLength(14, MinimumLength = 11)]
        public string? Cpf { get; set; }

        [StringLength(20)]
        public string? Telefone { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        public bool? Ativo { get; set; }
    }

    public class ResponsavelDto
    {
        public int Id { get; set; }
        public int UnidadeId { get; set; }
        public string UnidadeNome { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool Ativo { get; set; }
    }
}
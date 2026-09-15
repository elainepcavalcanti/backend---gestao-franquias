using System;
using System.ComponentModel.DataAnnotations;

namespace Franquias.Api.DTOs
{
    public class AtualizarFranqueadoraDto
    {
        [StringLength(150)]
        public string? Nome { get; set; }

        [StringLength(150)]
        public string? NomeFantasia { get; set; }

        [StringLength(150)]
        public string? RazaoSocial { get; set; }

        [StringLength(18, MinimumLength = 14, ErrorMessage = "Informe um CNPJ válido.")]
        public string? Cnpj { get; set; }

        [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
        [StringLength(100)]
        public string? Email { get; set; }
    }

    public class FranqueadoraDto
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string NomeFantasia { get; set; } = string.Empty;

        public string RazaoSocial { get; set; } = string.Empty;

        public string Cnpj { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public bool Ativa { get; set; }

        public DateTime DataCadastro { get; set; }
    }
}
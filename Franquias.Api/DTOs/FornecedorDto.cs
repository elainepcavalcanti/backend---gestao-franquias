using System.ComponentModel.DataAnnotations;

namespace Franquias.Api.DTOs
{
    public class CriarFornecedorDto
    {
        [Required]
        public string Nome { get; set; } = string.Empty;

        [Required]
        public string Cnpj { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Telefone { get; set; } = string.Empty;
    }

    public class AtualizarFornecedorDto
    {
        public string? Nome { get; set; }
        public string? Cnpj { get; set; }
        public string? Email { get; set; }
        public string? Telefone { get; set; }
        public bool? Ativo { get; set; }
    }

    public class FornecedorDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Cnpj { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public bool Ativo { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace Franquias.Api.DTOs
{
    public class CriarFranqueadoraDto
    {
        [Required(ErrorMessage = "Informe o nome.")]
        [StringLength(150, ErrorMessage = "O nome deve possuir no máximo 150 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [StringLength(150, ErrorMessage = "O nome fantasia deve possuir no máximo 150 caracteres.")]
        public string? NomeFantasia { get; set; }

        [StringLength(150, ErrorMessage = "A razão social deve possuir no máximo 150 caracteres.")]
        public string? RazaoSocial { get; set; }

        [Required(ErrorMessage = "Informe o CNPJ.")]
        [StringLength(18, MinimumLength = 14, ErrorMessage = "Informe um CNPJ válido.")]
        public string Cnpj { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o e-mail.")]
        [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
        [StringLength(100, ErrorMessage = "O e-mail deve possuir no máximo 100 caracteres.")]
        public string Email { get; set; } = string.Empty;
    }
}
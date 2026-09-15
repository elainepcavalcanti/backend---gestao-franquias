using System.ComponentModel.DataAnnotations;

namespace Franquias.Api.DTOs
{
    public class CriarAdministradorInicialDto
    {
        [Required(ErrorMessage = "Informe o nome.")]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o e-mail.")]
        [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe a senha.")]
        [MinLength(8, ErrorMessage = "A senha deve possuir pelo menos 8 caracteres.")]
        public string Senha { get; set; } = string.Empty;
    }
}
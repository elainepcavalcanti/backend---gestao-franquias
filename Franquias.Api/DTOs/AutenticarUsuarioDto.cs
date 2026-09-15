using System.ComponentModel.DataAnnotations;

namespace Franquias.Api.DTOs
{
    public class AutenticarUsuarioDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Senha { get; set; } = string.Empty;
    }

    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public UsuarioResumoDto Usuario { get; set; } =
            new UsuarioResumoDto();
    }

    public class UsuarioResumoDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Perfil { get; set; } = string.Empty;
        public int? UnidadeId { get; set; }
        public bool Ativo { get; set; }
    }
}
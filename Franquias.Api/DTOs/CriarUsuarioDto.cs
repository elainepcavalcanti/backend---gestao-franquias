using System.ComponentModel.DataAnnotations;

namespace Franquias.Api.DTOs
{
    public class CriarUsuarioDto
    {
        [Required]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Senha { get; set; } = string.Empty;

        [Required]
        public PerfilUsuario Perfil { get; set; }

        public int? UnidadeId { get; set; }

        public int? FranqueadoraId { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Franquias.Api
{
    [Table("Responsavel")]
    public class Responsavel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [MaxLength(14)]
        [Column("CPF")]
        public string Cpf { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Telefone { get; set; } = string.Empty;

        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public bool Ativo { get; set; } = true;

        public int UnidadeId { get; set; }

        [ForeignKey(nameof(UnidadeId))]
        public Unidade? Unidade { get; set; }
    }
}
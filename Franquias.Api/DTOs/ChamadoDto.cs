using System;
using System.ComponentModel.DataAnnotations;

namespace Franquias.Api.DTOs
{
    public class CriarChamadoDto
    {
        [Required(ErrorMessage = "Informe o título.")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe a descrição.")]
        public string Descricao { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe a categoria.")]
        public string Categoria { get; set; } = string.Empty;

        public PrioridadeChamado Prioridade { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Informe uma unidade válida.")]
        public int UnidadeId { get; set; }
    }

    public class AtualizarChamadoDto
    {
        public string? Titulo { get; set; }

        public string? Descricao { get; set; }

        public string? Categoria { get; set; }

        public PrioridadeChamado? Prioridade { get; set; }

        public StatusChamado? Status { get; set; }
    }

    public class ChamadoDto
    {
        public int Id { get; set; }

        public string Titulo { get; set; } = string.Empty;

        public string Descricao { get; set; } = string.Empty;

        public string Categoria { get; set; } = string.Empty;

        public PrioridadeChamado Prioridade { get; set; }

        public StatusChamado Status { get; set; }

        public int UnidadeId { get; set; }

        public string UnidadeNome { get; set; } = string.Empty;

        public int UsuarioId { get; set; }

        public string UsuarioNome { get; set; } = string.Empty;

        public DateTime DataAbertura { get; set; }

        public DateTime? DataFechamento { get; set; }
    }
}
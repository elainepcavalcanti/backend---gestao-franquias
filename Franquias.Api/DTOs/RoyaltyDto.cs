using System;
using System.ComponentModel.DataAnnotations;

namespace Franquias.Api.DTOs
{
    public class CriarRoyaltyConfigDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int UnidadeId { get; set; }

        [Range(0.01, 100)]
        public decimal Percentual { get; set; }
    }

    public class GerarRoyaltyDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int UnidadeId { get; set; }

        [Range(1, 12)]
        public int MesReferencia { get; set; }

        [Range(2000, 2100)]
        public int AnoReferencia { get; set; }
    }

    public class AtualizarStatusRoyaltyDto
    {
        [Required]
        public string Status { get; set; } =
            string.Empty;
    }

    public class RoyaltyConfigDto
    {
        public int Id { get; set; }
        public int UnidadeId { get; set; }
        public string UnidadeNome { get; set; } =
            string.Empty;
        public int MesReferencia { get; set; }
        public int AnoReferencia { get; set; }
        public decimal Faturamento { get; set; }
        public decimal Percentual { get; set; }
        public decimal ValorDevido { get; set; }
        public string StatusPagamento { get; set; } =
            string.Empty;
    }

    public class ResumoRoyaltyDto
    {
        public int UnidadeId { get; set; }
        public DateTime Inicio { get; set; }
        public DateTime Fim { get; set; }
        public decimal TotalGerado { get; set; }
        public decimal TotalPago { get; set; }
        public decimal TotalPendente { get; set; }
    }
}
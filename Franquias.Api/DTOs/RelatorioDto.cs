namespace Franquias.Api.DTOs
{
    public class FaturamentoUnidadeDto
    {
        public int UnidadeId { get; set; }
        public string UnidadeNome { get; set; } =
            string.Empty;
        public decimal Faturamento { get; set; }
    }

    public class RankingUnidadeDto
    {
        public int Posicao { get; set; }
        public int UnidadeId { get; set; }
        public string UnidadeNome { get; set; } =
            string.Empty;
        public decimal Faturamento { get; set; }
    }

    public class RoyaltyUnidadeDto
    {
        public int UnidadeId { get; set; }
        public string UnidadeNome { get; set; } =
            string.Empty;
        public decimal TotalGerado { get; set; }
        public decimal TotalPago { get; set; }
        public decimal TotalPendente { get; set; }
    }

    public class ProdutoMaisVendidoDto
    {
        public int ProdutoId { get; set; }
        public string ProdutoNome { get; set; } =
            string.Empty;
        public int QuantidadeVendida { get; set; }
        public decimal Faturamento { get; set; }
    }

    public class ChamadosPorStatusDto
    {
        public string Status { get; set; } =
            string.Empty;
        public int Quantidade { get; set; }
    }
}
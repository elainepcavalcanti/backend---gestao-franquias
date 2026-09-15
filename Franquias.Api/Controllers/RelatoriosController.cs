using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Franquias.Api.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Franquias.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RelatoriosController : ControllerBase
    {
        private readonly FranquiasDbContext _db;

        public RelatoriosController(
            FranquiasDbContext db)
        {
            _db = db;
        }

        [HttpGet("faturamento")]
        [Authorize(Roles = "Administrador,Gestor")]
        public async Task<ActionResult<
            List<FaturamentoUnidadeDto>>> Faturamento(
                [FromQuery] int? franqueadoraId,
                [FromQuery] int? unidadeId,
                [FromQuery] DateTime? inicio,
                [FromQuery] DateTime? fim)
        {
            if (PeriodoInvalido(inicio, fim))
            {
                return BadRequest(
                    "A data final não pode ser anterior à data inicial.");
            }

            var resultado = await FiltrarVendas(
                    franqueadoraId,
                    unidadeId,
                    inicio,
                    fim)
                .GroupBy(v => new
                {
                    v.UnidadeId,
                    UnidadeNome = v.Unidade != null
                        ? v.Unidade.Nome
                        : string.Empty
                })
                .Select(g => new FaturamentoUnidadeDto
                {
                    UnidadeId = g.Key.UnidadeId,
                    UnidadeNome =
                        g.Key.UnidadeNome,
                    Faturamento =
                        g.Sum(v => v.Total)
                })
                .OrderByDescending(r =>
                    r.Faturamento)
                .ToListAsync();

            return Ok(resultado);
        }

        [HttpGet("ranking-unidades")]
        [Authorize(Roles = "Administrador,Gestor")]
        public async Task<ActionResult<
            List<RankingUnidadeDto>>> RankingUnidades(
                [FromQuery] int? franqueadoraId,
                [FromQuery] DateTime? inicio,
                [FromQuery] DateTime? fim)
        {
            if (PeriodoInvalido(inicio, fim))
            {
                return BadRequest(
                    "A data final não pode ser anterior à data inicial.");
            }

            var faturamentos = await FiltrarVendas(
                    franqueadoraId,
                    null,
                    inicio,
                    fim)
                .GroupBy(v => new
                {
                    v.UnidadeId,
                    UnidadeNome = v.Unidade != null
                        ? v.Unidade.Nome
                        : string.Empty
                })
                .Select(g => new FaturamentoUnidadeDto
                {
                    UnidadeId = g.Key.UnidadeId,
                    UnidadeNome =
                        g.Key.UnidadeNome,
                    Faturamento =
                        g.Sum(v => v.Total)
                })
                .OrderByDescending(r =>
                    r.Faturamento)
                .ToListAsync();

            var ranking = faturamentos
                .Select((item, indice) =>
                    new RankingUnidadeDto
                    {
                        Posicao = indice + 1,
                        UnidadeId =
                            item.UnidadeId,
                        UnidadeNome =
                            item.UnidadeNome,
                        Faturamento =
                            item.Faturamento
                    })
                .ToList();

            return Ok(ranking);
        }

        [HttpGet("royalties")]
        [Authorize(Roles = "Administrador,Gestor")]
        public async Task<ActionResult<
            List<RoyaltyUnidadeDto>>> Royalties(
                [FromQuery] int? franqueadoraId,
                [FromQuery] int? unidadeId,
                [FromQuery] DateTime? inicio,
                [FromQuery] DateTime? fim)
        {
            if (PeriodoInvalido(inicio, fim))
            {
                return BadRequest(
                    "A data final não pode ser anterior à data inicial.");
            }

            var query = _db.RoyaltyConfigs
                .AsNoTracking()
                .Where(r =>
                    r.StatusPagamento !=
                        "Cancelado")
                .AsQueryable();

            if (franqueadoraId.HasValue)
            {
                query = query.Where(r =>
                    r.Unidade != null &&
                    r.Unidade.FranqueadoraId ==
                        franqueadoraId.Value);
            }

            if (unidadeId.HasValue)
            {
                query = query.Where(r =>
                    r.UnidadeId ==
                        unidadeId.Value);
            }

            if (inicio.HasValue)
            {
                var chaveInicial =
                    inicio.Value.Year * 100 +
                    inicio.Value.Month;

                query = query.Where(r =>
                    r.AnoReferencia * 100 +
                    r.MesReferencia >=
                        chaveInicial);
            }

            if (fim.HasValue)
            {
                var chaveFinal =
                    fim.Value.Year * 100 +
                    fim.Value.Month;

                query = query.Where(r =>
                    r.AnoReferencia * 100 +
                    r.MesReferencia <=
                        chaveFinal);
            }

            var resultado = await query
                .GroupBy(r => new
                {
                    r.UnidadeId,
                    UnidadeNome = r.Unidade != null
                        ? r.Unidade.Nome
                        : string.Empty
                })
                .Select(g => new RoyaltyUnidadeDto
                {
                    UnidadeId = g.Key.UnidadeId,
                    UnidadeNome =
                        g.Key.UnidadeNome,
                    TotalGerado =
                        g.Sum(r => r.ValorDevido),
                    TotalPago = g.Sum(r =>
                        r.StatusPagamento == "Pago"
                            ? r.ValorDevido
                            : 0m),
                    TotalPendente = g.Sum(r =>
                        r.StatusPagamento ==
                            "Pendente" ||
                        r.StatusPagamento ==
                            "Atrasado"
                            ? r.ValorDevido
                            : 0m)
                })
                .OrderByDescending(r =>
                    r.TotalGerado)
                .ToListAsync();

            return Ok(resultado);
        }

        [HttpGet("produtos-mais-vendidos")]
        [Authorize(Roles = "Administrador,Gestor")]
        public async Task<ActionResult<
            List<ProdutoMaisVendidoDto>>>
            ProdutosMaisVendidos(
                [FromQuery] int? franqueadoraId,
                [FromQuery] int? unidadeId,
                [FromQuery] DateTime? inicio,
                [FromQuery] DateTime? fim,
                [FromQuery] int limite = 10)
        {
            if (PeriodoInvalido(inicio, fim))
            {
                return BadRequest(
                    "A data final não pode ser anterior à data inicial.");
            }

            if (limite < 1 || limite > 100)
            {
                return BadRequest(
                    "O limite deve estar entre 1 e 100.");
            }

            var query = _db.ItensVenda
                .AsNoTracking()
                .AsQueryable();

            if (franqueadoraId.HasValue)
            {
                query = query.Where(i =>
                    i.Venda != null &&
                    i.Venda.Unidade != null &&
                    i.Venda.Unidade
                        .FranqueadoraId ==
                            franqueadoraId.Value);
            }

            if (unidadeId.HasValue)
            {
                query = query.Where(i =>
                    i.Venda != null &&
                    i.Venda.UnidadeId ==
                        unidadeId.Value);
            }

            if (inicio.HasValue)
            {
                var dataInicial =
                    inicio.Value.Date;

                query = query.Where(i =>
                    i.Venda != null &&
                    i.Venda.DataVenda >=
                        dataInicial);
            }

            if (fim.HasValue)
            {
                var dataFinalExclusiva =
                    fim.Value.Date.AddDays(1);

                query = query.Where(i =>
                    i.Venda != null &&
                    i.Venda.DataVenda <
                        dataFinalExclusiva);
            }

            var resultado = await query
                .GroupBy(i => new
                {
                    i.ProdutoId,
                    ProdutoNome = i.Produto != null
                        ? i.Produto.Nome
                        : string.Empty
                })
                .Select(g =>
                    new ProdutoMaisVendidoDto
                    {
                        ProdutoId =
                            g.Key.ProdutoId,
                        ProdutoNome =
                            g.Key.ProdutoNome,
                        QuantidadeVendida =
                            g.Sum(i =>
                                i.Quantidade),
                        Faturamento =
                            g.Sum(i =>
                                i.Subtotal)
                    })
                .OrderByDescending(r =>
                    r.QuantidadeVendida)
                .Take(limite)
                .ToListAsync();

            return Ok(resultado);
        }

        [HttpGet("estoque-critico")]
        [Authorize(Roles = "Administrador,Gestor")]
        public async Task<ActionResult<
            List<EstoqueResumoDto>>> EstoqueCritico(
                [FromQuery] int? franqueadoraId,
                [FromQuery] int? unidadeId)
        {
            var query = _db.Estoques
                .AsNoTracking()
                .Where(e =>
                    e.Produto != null &&
                    e.Quantidade <=
                        e.Produto.EstoqueMinimo)
                .AsQueryable();

            if (franqueadoraId.HasValue)
            {
                query = query.Where(e =>
                    e.Unidade != null &&
                    e.Unidade.FranqueadoraId ==
                        franqueadoraId.Value);
            }

            if (unidadeId.HasValue)
            {
                query = query.Where(e =>
                    e.UnidadeId ==
                        unidadeId.Value);
            }

            var resultado = await query
                .Select(e => new EstoqueResumoDto
                {
                    ProdutoId = e.ProdutoId,
                    ProdutoNome =
                        e.Produto != null
                            ? e.Produto.Nome
                            : string.Empty,
                    UnidadeId = e.UnidadeId,
                    UnidadeNome =
                        e.Unidade != null
                            ? e.Unidade.Nome
                            : string.Empty,
                    Quantidade = e.Quantidade,
                    EstoqueMinimo =
                        e.Produto != null
                            ? e.Produto
                                .EstoqueMinimo
                            : 0
                })
                .ToListAsync();

            return Ok(resultado);
        }

        [HttpGet("chamados-por-status")]
        [Authorize(Roles = "Administrador,Gestor")]
        public async Task<ActionResult<
            List<ChamadosPorStatusDto>>>
            ChamadosPorStatus(
                [FromQuery] int? franqueadoraId,
                [FromQuery] int? unidadeId,
                [FromQuery] PrioridadeChamado?
                    prioridade,
                [FromQuery] bool somenteAbertos =
                    false)
        {
            var query = _db.Chamados
                .AsNoTracking()
                .AsQueryable();

            if (franqueadoraId.HasValue)
            {
                query = query.Where(c =>
                    c.Unidade != null &&
                    c.Unidade.FranqueadoraId ==
                        franqueadoraId.Value);
            }

            if (unidadeId.HasValue)
            {
                query = query.Where(c =>
                    c.UnidadeId ==
                        unidadeId.Value);
            }

            if (prioridade.HasValue)
            {
                query = query.Where(c =>
                    c.Prioridade ==
                        prioridade.Value);
            }

            if (somenteAbertos)
            {
                query = query.Where(c =>
                    c.Status !=
                        StatusChamado.Resolvido &&
                    c.Status !=
                        StatusChamado.Fechado);
            }

            var agrupados = await query
                .GroupBy(c => c.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Quantidade = g.Count()
                })
                .ToListAsync();

            var resultado = agrupados
                .Select(item =>
                    new ChamadosPorStatusDto
                    {
                        Status =
                            item.Status.ToString(),
                        Quantidade =
                            item.Quantidade
                    })
                .ToList();

            return Ok(resultado);
        }

        private IQueryable<Venda> FiltrarVendas(
            int? franqueadoraId,
            int? unidadeId,
            DateTime? inicio,
            DateTime? fim)
        {
            var query = _db.Vendas
                .AsNoTracking()
                .AsQueryable();

            if (franqueadoraId.HasValue)
            {
                query = query.Where(v =>
                    v.Unidade != null &&
                    v.Unidade.FranqueadoraId ==
                        franqueadoraId.Value);
            }

            if (unidadeId.HasValue)
            {
                query = query.Where(v =>
                    v.UnidadeId ==
                        unidadeId.Value);
            }

            if (inicio.HasValue)
            {
                var dataInicial =
                    inicio.Value.Date;

                query = query.Where(v =>
                    v.DataVenda >= dataInicial);
            }

            if (fim.HasValue)
            {
                var dataFinalExclusiva =
                    fim.Value.Date.AddDays(1);

                query = query.Where(v =>
                    v.DataVenda <
                        dataFinalExclusiva);
            }

            return query;
        }

        private static bool PeriodoInvalido(
            DateTime? inicio,
            DateTime? fim)
        {
            return inicio.HasValue &&
                   fim.HasValue &&
                   fim.Value.Date <
                       inicio.Value.Date;
        }
    }
}
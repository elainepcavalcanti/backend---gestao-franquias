using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Franquias.Api.DTOs;
using Franquias.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Franquias.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoyaltyController : ControllerBase
    {
        private readonly FranquiasDbContext _db;
        private readonly IRoyaltyCalculatorService _royaltyCalculator;

        public RoyaltyController(
            FranquiasDbContext db,
            IRoyaltyCalculatorService royaltyCalculator)
        {
            _db = db;
            _royaltyCalculator = royaltyCalculator;
        }

        [HttpGet]
        [Authorize(Roles = "Administrador,Gestor")]
        public async Task<ActionResult<List<RoyaltyConfigDto>>>
            ObterCobrancas(
                [FromQuery] int? unidadeId,
                [FromQuery] int? mes,
                [FromQuery] int? ano,
                [FromQuery] string? status,
                [FromQuery] int pagina = 1,
                [FromQuery] int tamanhoPagina = 20)
        {
            if (pagina < 1)
            {
                return BadRequest(
                    "A página deve ser maior ou igual a 1.");
            }

            if (tamanhoPagina < 1 || tamanhoPagina > 100)
            {
                return BadRequest(
                    "O tamanho da página deve estar entre 1 e 100.");
            }

            var query = _db.RoyaltyConfigs
                .AsNoTracking()
                .AsQueryable();

            if (unidadeId.HasValue)
            {
                query = query.Where(
                    r => r.UnidadeId == unidadeId.Value);
            }

            if (mes.HasValue)
            {
                query = query.Where(
                    r => r.MesReferencia == mes.Value);
            }

            if (ano.HasValue)
            {
                query = query.Where(
                    r => r.AnoReferencia == ano.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                var termo = status.Trim();

                query = query.Where(
                    r => r.StatusPagamento == termo);
            }

            var total = await query.CountAsync();

            Response.Headers["X-Total-Count"] =
                total.ToString();

            var cobrancas = await query
                .OrderByDescending(r => r.AnoReferencia)
                .ThenByDescending(r => r.MesReferencia)
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .Select(r => new RoyaltyConfigDto
                {
                    Id = r.Id,
                    UnidadeId = r.UnidadeId,
                    UnidadeNome = r.Unidade != null
                        ? r.Unidade.Nome
                        : string.Empty,
                    MesReferencia = r.MesReferencia,
                    AnoReferencia = r.AnoReferencia,
                    Faturamento =
                        r.FaturamentoTotalPeriodo,
                    Percentual = r.Percentual,
                    ValorDevido = r.ValorDevido,
                    StatusPagamento =
                        r.StatusPagamento
                })
                .ToListAsync();

            return Ok(cobrancas);
        }

        [HttpPut("configuracao")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult>
            ConfigurarPercentual(
                [FromBody] CriarRoyaltyConfigDto dto)
        {
            if (dto.Percentual <= 0 ||
                dto.Percentual > 100)
            {
                return BadRequest(
                    "O percentual deve estar entre 0,01 e 100.");
            }

            var unidade = await _db.Unidades
                .FirstOrDefaultAsync(u =>
                    u.Id == dto.UnidadeId);

            if (unidade == null)
            {
                return NotFound(
                    "Unidade não encontrada.");
            }

            unidade.PercentualRoyalty =
                dto.Percentual;

            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("calcular")]
        [Authorize(Roles = "Administrador,Gestor")]
        public async Task<ActionResult<RoyaltyConfigDto>>
            CalcularESalvar(
                [FromBody] GerarRoyaltyDto dto)
        {
            var unidade = await _db.Unidades
                .AsNoTracking()
                .FirstOrDefaultAsync(u =>
                    u.Id == dto.UnidadeId);

            if (unidade == null)
            {
                return NotFound(
                    "Unidade não encontrada.");
            }

            if (unidade.PercentualRoyalty <= 0 ||
                unidade.PercentualRoyalty > 100)
            {
                return BadRequest(
                    "Configure um percentual de royalty válido para a unidade.");
            }

            var cobrancaExistente =
                await _db.RoyaltyConfigs.AnyAsync(r =>
                    r.UnidadeId == dto.UnidadeId &&
                    r.MesReferencia ==
                        dto.MesReferencia &&
                    r.AnoReferencia ==
                        dto.AnoReferencia);

            if (cobrancaExistente)
            {
                return Conflict(
                    "Já existe uma cobrança de royalty para esta unidade e período.");
            }

            var inicio = new DateTime(
                dto.AnoReferencia,
                dto.MesReferencia,
                1,
                0,
                0,
                0,
                DateTimeKind.Utc);

            var fimExclusivo = inicio.AddMonths(1);

            var faturamento = await _db.Vendas
                .Where(v =>
                    v.UnidadeId == dto.UnidadeId &&
                    v.DataVenda >= inicio &&
                    v.DataVenda < fimExclusivo)
                .Select(v => (decimal?)v.Total)
                .SumAsync() ?? 0m;

            var valorDevido = _royaltyCalculator.Calcular(
                faturamento,
                unidade.PercentualRoyalty);

            var cobranca = new RoyaltyConfig
            {
                UnidadeId = dto.UnidadeId,
                MesReferencia = dto.MesReferencia,
                AnoReferencia = dto.AnoReferencia,
                FaturamentoTotalPeriodo =
                    Math.Round(faturamento, 2),
                Percentual =
                    unidade.PercentualRoyalty,
                ValorDevido = valorDevido,
                StatusPagamento = "Pendente"
            };

            _db.RoyaltyConfigs.Add(cobranca);
            await _db.SaveChangesAsync();

            var resposta = new RoyaltyConfigDto
            {
                Id = cobranca.Id,
                UnidadeId = cobranca.UnidadeId,
                UnidadeNome = unidade.Nome,
                MesReferencia =
                    cobranca.MesReferencia,
                AnoReferencia =
                    cobranca.AnoReferencia,
                Faturamento =
                    cobranca.FaturamentoTotalPeriodo,
                Percentual =
                    cobranca.Percentual,
                ValorDevido =
                    cobranca.ValorDevido,
                StatusPagamento =
                    cobranca.StatusPagamento
            };

            return CreatedAtAction(
                nameof(ObterCobrancas),
                new { unidadeId = dto.UnidadeId },
                resposta);
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<RoyaltyConfigDto>>
            AtualizarStatus(
                int id,
                [FromBody] AtualizarStatusRoyaltyDto dto)
        {
            var cobranca = await _db.RoyaltyConfigs
                .Include(r => r.Unidade)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (cobranca == null)
            {
                return NotFound(
                    "Cobrança de royalty não encontrada.");
            }

            var statusNormalizado =
                NormalizarStatus(dto.Status);

            if (statusNormalizado == null)
            {
                return BadRequest(
                    "Status inválido. Utilize Pendente, Pago, Atrasado ou Cancelado.");
            }

            cobranca.StatusPagamento =
                statusNormalizado;

            await _db.SaveChangesAsync();

            return Ok(new RoyaltyConfigDto
            {
                Id = cobranca.Id,
                UnidadeId = cobranca.UnidadeId,
                UnidadeNome =
                    cobranca.Unidade?.Nome ??
                    string.Empty,
                MesReferencia =
                    cobranca.MesReferencia,
                AnoReferencia =
                    cobranca.AnoReferencia,
                Faturamento =
                    cobranca.FaturamentoTotalPeriodo,
                Percentual =
                    cobranca.Percentual,
                ValorDevido =
                    cobranca.ValorDevido,
                StatusPagamento =
                    cobranca.StatusPagamento
            });
        }

        [HttpGet("resumo")]
        [Authorize(Roles = "Administrador,Gestor")]
        public async Task<ActionResult<ResumoRoyaltyDto>>
            ObterResumo(
                [FromQuery] int unidadeId,
                [FromQuery] DateTime inicio,
                [FromQuery] DateTime fim)
        {
            if (fim.Date < inicio.Date)
            {
                return BadRequest(
                    "A data final não pode ser anterior à data inicial.");
            }

            var unidadeExiste =
                await _db.Unidades.AnyAsync(u =>
                    u.Id == unidadeId);

            if (!unidadeExiste)
            {
                return NotFound(
                    "Unidade não encontrada.");
            }

            var chaveInicial =
                inicio.Year * 100 + inicio.Month;

            var chaveFinal =
                fim.Year * 100 + fim.Month;

            var query = _db.RoyaltyConfigs
                .AsNoTracking()
                .Where(r =>
                    r.UnidadeId == unidadeId &&
                    r.AnoReferencia * 100 +
                        r.MesReferencia >= chaveInicial &&
                    r.AnoReferencia * 100 +
                        r.MesReferencia <= chaveFinal &&
                    r.StatusPagamento != "Cancelado");

            var totalGerado = await query
                .Select(r => (decimal?)r.ValorDevido)
                .SumAsync() ?? 0m;

            var totalPago = await query
                .Where(r =>
                    r.StatusPagamento == "Pago")
                .Select(r => (decimal?)r.ValorDevido)
                .SumAsync() ?? 0m;

            var totalPendente = await query
                .Where(r =>
                    r.StatusPagamento == "Pendente" ||
                    r.StatusPagamento == "Atrasado")
                .Select(r => (decimal?)r.ValorDevido)
                .SumAsync() ?? 0m;

            return Ok(new ResumoRoyaltyDto
            {
                UnidadeId = unidadeId,
                Inicio = inicio.Date,
                Fim = fim.Date,
                TotalGerado =
                    Math.Round(totalGerado, 2),
                TotalPago =
                    Math.Round(totalPago, 2),
                TotalPendente =
                    Math.Round(totalPendente, 2)
            });
        }

        private static string? NormalizarStatus(
            string status)
        {
            return status.Trim().ToLowerInvariant()
                switch
                {
                    "pendente" => "Pendente",
                    "pago" => "Pago",
                    "atrasado" => "Atrasado",
                    "cancelado" => "Cancelado",
                    _ => null
                };
        }
    }
}
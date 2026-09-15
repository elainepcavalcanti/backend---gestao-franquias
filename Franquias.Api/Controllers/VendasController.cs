using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Franquias.Api.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Franquias.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VendasController : ControllerBase
    {
        private readonly FranquiasDbContext _db;

        public VendasController(FranquiasDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        [Authorize(Roles = "Administrador,Gestor,Operador")]
        public async Task<ActionResult<List<VendaDto>>> ObterTodos(
            [FromQuery] int? unidadeId,
            [FromQuery] DateTime? dataInicial,
            [FromQuery] DateTime? dataFinal,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanhoPagina = 20,
            [FromQuery] string ordenarPor = "data",
            [FromQuery] bool decrescente = true)
        {
            if (pagina < 1)
            {
                return BadRequest(
                    "A página deve ser maior ou igual a 1.");
            }

            if (tamanhoPagina < 1 ||
                tamanhoPagina > 100)
            {
                return BadRequest(
                    "O tamanho da página deve estar entre 1 e 100.");
            }

            if (dataInicial.HasValue &&
                dataFinal.HasValue &&
                dataFinal.Value.Date <
                dataInicial.Value.Date)
            {
                return BadRequest(
                    "A data final não pode ser anterior à data inicial.");
            }

            var query = _db.Vendas
                .AsNoTracking()
                .AsQueryable();

            if (unidadeId.HasValue)
            {
                query = query.Where(
                    v => v.UnidadeId == unidadeId.Value);
            }

            if (dataInicial.HasValue)
            {
                var inicio = dataInicial.Value.Date;

                query = query.Where(
                    v => v.DataVenda >= inicio);
            }

            if (dataFinal.HasValue)
            {
                var fimExclusivo =
                    dataFinal.Value.Date.AddDays(1);

                query = query.Where(
                    v => v.DataVenda < fimExclusivo);
            }

            var total = await query.CountAsync();

            Response.Headers["X-Total-Count"] =
                total.ToString();

            switch (ordenarPor.Trim().ToLowerInvariant())
            {
                case "id":
                    query = decrescente
                        ? query.OrderByDescending(v => v.Id)
                        : query.OrderBy(v => v.Id);
                    break;

                case "total":
                    query = decrescente
                        ? query.OrderByDescending(v => v.Total)
                        : query.OrderBy(v => v.Total);
                    break;

                default:
                    query = decrescente
                        ? query.OrderByDescending(
                            v => v.DataVenda)
                        : query.OrderBy(v => v.DataVenda);
                    break;
            }

            var vendas = await query
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .Select(v => new VendaDto
                {
                    Id = v.Id,
                    UnidadeId = v.UnidadeId,
                    UsuarioId = v.UsuarioId,
                    DataVenda = v.DataVenda,
                    Total = v.Total,
                    Itens = v.Itens
                        .Select(i => new ItemVendaDto
                        {
                            ProdutoId = i.ProdutoId,
                            Quantidade = i.Quantidade
                        })
                        .ToList()
                })
                .ToListAsync();

            return Ok(vendas);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Gestor,Operador")]
        public async Task<ActionResult<VendaDto>> Criar(
            [FromBody] CriarVendaDto dto)
        {
            var idDoToken =
                User.FindFirst(
                    ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;

            if (!int.TryParse(
                    idDoToken,
                    out var usuarioId) ||
                usuarioId <= 0)
            {
                return Unauthorized(
                    "Não foi possível identificar o usuário autenticado.");
            }

            var usuario = await _db.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u =>
                    u.Id == usuarioId &&
                    u.Ativo);

            if (usuario == null)
            {
                return Unauthorized(
                    "Usuário não encontrado ou inativo.");
            }

            if (dto.UnidadeId <= 0)
            {
                return BadRequest(
                    "Informe uma unidade válida.");
            }

            if (dto.Itens == null ||
                dto.Itens.Count == 0)
            {
                return BadRequest(
                    "A venda precisa conter itens.");
            }

            if (dto.Itens.Any(i =>
                i == null ||
                i.ProdutoId <= 0 ||
                i.Quantidade <= 0))
            {
                return BadRequest(
                    "Todos os itens precisam ter produto e quantidade válidos.");
            }

            var grupos = dto.Itens
                .GroupBy(i => i.ProdutoId)
                .Select(g => new
                {
                    ProdutoId = g.Key,
                    Quantidade = g.Sum(
                        i => (long)i.Quantidade)
                })
                .OrderBy(g => g.ProdutoId)
                .ToList();

            if (grupos.Any(g =>
                g.Quantidade > int.MaxValue))
            {
                return BadRequest(
                    "A quantidade total de um produto excede o limite permitido.");
            }

            var itensAgrupados = grupos
                .Select(g => new ItemVendaDto
                {
                    ProdutoId = g.ProdutoId,
                    Quantidade = (int)g.Quantidade
                })
                .ToList();

            var unidade = await _db.Unidades
                .AsNoTracking()
                .FirstOrDefaultAsync(u =>
                    u.Id == dto.UnidadeId);

            if (unidade == null)
            {
                return NotFound(
                    "Unidade não encontrada.");
            }

            if (!unidade.Ativa)
            {
                return BadRequest(
                    "Não é permitido vender para uma unidade inativa.");
            }

            await using var transacao =
                await _db.Database
                    .BeginTransactionAsync();

            var dataVenda = DateTime.UtcNow;

            var venda = new Venda
            {
                UnidadeId = dto.UnidadeId,
                UsuarioId = usuario.Id,
                DataVenda = dataVenda,
                Total = 0
            };

            decimal totalVenda = 0;
            var itens = new List<ItemVenda>();

            foreach (var itemDto in itensAgrupados)
            {
                var produto = await _db.Produtos
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p =>
                        p.Id == itemDto.ProdutoId);

                if (produto == null)
                {
                    return BadRequest(
                        $"Produto {itemDto.ProdutoId} não encontrado.");
                }

                if (!produto.Ativo)
                {
                    return BadRequest(
                        $"Produto {produto.Nome} não está ativo.");
                }

                if (produto.Preco <= 0)
                {
                    return BadRequest(
                        $"O produto {produto.Nome} possui preço inválido.");
                }

                var estoqueId = await _db.Estoques
                    .Where(e =>
                        e.ProdutoId ==
                            itemDto.ProdutoId &&
                        e.UnidadeId ==
                            dto.UnidadeId)
                    .Select(e => (int?)e.Id)
                    .SingleOrDefaultAsync();

                if (!estoqueId.HasValue)
                {
                    return BadRequest(
                        $"Estoque insuficiente para o produto {produto.Nome}.");
                }

                var quantidade = itemDto.Quantidade;

                var linhasAtualizadas =
                    await _db.Estoques
                        .Where(e =>
                            e.Id == estoqueId.Value &&
                            e.Quantidade >= quantidade)
                        .ExecuteUpdateAsync(setters =>
                            setters.SetProperty(
                                e => e.Quantidade,
                                e => e.Quantidade -
                                     quantidade));

                if (linhasAtualizadas != 1)
                {
                    return BadRequest(
                        $"Estoque insuficiente para o produto {produto.Nome}.");
                }

                var subtotal =
                    produto.Preco *
                    itemDto.Quantidade;

                totalVenda += subtotal;

                itens.Add(new ItemVenda
                {
                    ProdutoId = itemDto.ProdutoId,
                    Quantidade = itemDto.Quantidade,
                    PrecoUnitario = produto.Preco,
                    Subtotal = subtotal
                });

                _db.MovimentosEstoque.Add(
                    new MovimentoEstoque
                    {
                        EstoqueUnidadeId =
                            estoqueId.Value,
                        Tipo =
                            TipoMovimentoEstoque.Saida,
                        Quantidade = quantidade,
                        Motivo = "Venda",
                        DataMovimento = dataVenda
                    });
            }

            venda.Total = totalVenda;
            venda.Itens = itens;

            _db.Vendas.Add(venda);
            await _db.SaveChangesAsync();

            await transacao.CommitAsync();

            return CreatedAtAction(
                nameof(ObterTodos),
                new { unidadeId = dto.UnidadeId },
                new VendaDto
                {
                    Id = venda.Id,
                    UnidadeId = venda.UnidadeId,
                    UsuarioId = venda.UsuarioId,
                    DataVenda = venda.DataVenda,
                    Total = venda.Total,
                    Itens = itensAgrupados
                });
        }
    }
}
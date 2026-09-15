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
    public class EstoqueController : ControllerBase
    {
        private readonly FranquiasDbContext _db;

        public EstoqueController(FranquiasDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        [Authorize(Roles = "Administrador,Gestor,Operador")]
        public async Task<ActionResult<List<EstoqueResumoDto>>> ObterEstoque([FromQuery] int? unidadeId, [FromQuery] int? franqueadoraId)
        {
            var query = _db.Estoques
                .Include(e => e.Produto)
                .Include(e => e.Unidade)
                .AsQueryable();

            if (unidadeId.HasValue)
            {
                query = query.Where(e => e.UnidadeId == unidadeId.Value);
            }

            if (franqueadoraId.HasValue)
            {
                query = query.Where(e => e.Unidade != null && e.Unidade.FranqueadoraId == franqueadoraId.Value);
            }

            var estoque = await query
                .Select(e => new EstoqueResumoDto
                {
                    ProdutoId = e.ProdutoId,
                    ProdutoNome = e.Produto != null ? e.Produto.Nome : string.Empty,
                    UnidadeId = e.UnidadeId,
                    UnidadeNome = e.Unidade != null ? e.Unidade.Nome : string.Empty,
                    Quantidade = e.Quantidade,
                    EstoqueMinimo = e.Produto != null ? e.Produto.EstoqueMinimo : 0
                })
                .ToListAsync();

            return Ok(estoque);
        }

        [HttpGet("critico")]
        [Authorize(Roles = "Administrador,Gestor,Operador")]
        public async Task<ActionResult<List<EstoqueResumoDto>>> EstoqueCritico([FromQuery] int? franqueadoraId)
        {
            var query = _db.Estoques
                .Include(e => e.Produto)
                .Include(e => e.Unidade)
                .Where(e => e.Produto != null && e.Quantidade <= e.Produto.EstoqueMinimo)
                .AsQueryable();

            if (franqueadoraId.HasValue)
            {
                query = query.Where(e => e.Unidade != null && e.Unidade.FranqueadoraId == franqueadoraId.Value);
            }

            var result = await query
                .Select(e => new EstoqueResumoDto
                {
                    ProdutoId = e.ProdutoId,
                    ProdutoNome = e.Produto != null ? e.Produto.Nome : string.Empty,
                    UnidadeId = e.UnidadeId,
                    UnidadeNome = e.Unidade != null ? e.Unidade.Nome : string.Empty,
                    Quantidade = e.Quantidade,
                    EstoqueMinimo = e.Produto != null ? e.Produto.EstoqueMinimo : 0
                })
                .ToListAsync();

            return Ok(result);
        }

        [HttpPost("movimentar")]
        [Authorize(Roles = "Administrador,Gestor,Operador")]
        public async Task<ActionResult> Movimentar([FromBody] MovimentoEstoqueDto dto)
        {
            var produto = await _db.Produtos.FindAsync(dto.ProdutoId);
            if (produto == null)
            {
                return NotFound("Produto não encontrado.");
            }

            var unidade = await _db.Unidades.FindAsync(dto.UnidadeId);
            if (unidade == null)
            {
                return NotFound("Unidade não encontrada.");
            }

            if (!unidade.Ativa)
            {
                return BadRequest("Não é permitido movimentar estoque de uma unidade inativa.");
            }

            var estoque = await _db.Estoques
                .FirstOrDefaultAsync(e => e.ProdutoId == dto.ProdutoId && e.UnidadeId == dto.UnidadeId);

            if (estoque == null)
            {
                estoque = new EstoqueProduto
                {
                    ProdutoId = dto.ProdutoId,
                    UnidadeId = dto.UnidadeId,
                    Quantidade = 0
                };
                _db.Estoques.Add(estoque);
                await _db.SaveChangesAsync();
            }

            if (dto.Tipo == TipoMovimentoEstoque.Entrada)
            {
                estoque.Quantidade += dto.Quantidade;
            }
            else
            {
                if (estoque.Quantidade < dto.Quantidade)
                {
                    return BadRequest("Saldo insuficiente para realizar a saída do produto.");
                }

                estoque.Quantidade -= dto.Quantidade;
            }

            _db.MovimentosEstoque.Add(new MovimentoEstoque
            {
                EstoqueUnidadeId = estoque.Id,
                Tipo = dto.Tipo,
                Quantidade = dto.Quantidade,
                Motivo = dto.Motivo,
                DataMovimento = System.DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            return Ok(estoque);
        }
    }
}

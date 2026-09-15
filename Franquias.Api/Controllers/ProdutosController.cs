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
    public class ProdutosController : ControllerBase
    {
        private readonly FranquiasDbContext _db;

        public ProdutosController(FranquiasDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        [Authorize(Roles = "Administrador,Gestor,Operador")]
        public async Task<ActionResult<List<ProdutoDto>>> ObterTodos(
            [FromQuery] int? franqueadoraId,
            [FromQuery] string? nome,
            [FromQuery] int? categoriaId,
            [FromQuery] bool? ativo,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanhoPagina = 20,
            [FromQuery] string ordenarPor = "nome",
            [FromQuery] bool decrescente = false)
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

            var query = _db.Produtos
                .AsNoTracking()
                .AsQueryable();

            if (franqueadoraId.HasValue)
            {
                query = query.Where(p =>
                    _db.Estoques.Any(e =>
                        e.ProdutoId == p.Id &&
                        e.Unidade != null &&
                        e.Unidade.FranqueadoraId ==
                            franqueadoraId.Value));
            }

            if (!string.IsNullOrWhiteSpace(nome))
            {
                var termo = nome.Trim();

                query = query.Where(
                    p => p.Nome.Contains(termo));
            }

            if (categoriaId.HasValue)
            {
                query = query.Where(
                    p => p.CategoriaId ==
                         categoriaId.Value);
            }

            if (ativo.HasValue)
            {
                query = query.Where(
                    p => p.Ativo == ativo.Value);
            }

            var total = await query.CountAsync();
            Response.Headers["X-Total-Count"] =
                total.ToString();

            switch (ordenarPor.Trim().ToLowerInvariant())
            {
                case "id":
                    query = decrescente
                        ? query.OrderByDescending(p => p.Id)
                        : query.OrderBy(p => p.Id);
                    break;

                case "preco":
                    query = decrescente
                        ? query.OrderByDescending(p => p.Preco)
                        : query.OrderBy(p => p.Preco);
                    break;

                case "estoqueminimo":
                    query = decrescente
                        ? query.OrderByDescending(
                            p => p.EstoqueMinimo)
                        : query.OrderBy(
                            p => p.EstoqueMinimo);
                    break;

                default:
                    query = decrescente
                        ? query.OrderByDescending(p => p.Nome)
                        : query.OrderBy(p => p.Nome);
                    break;
            }

            var produtos = await query
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .Select(p => new ProdutoDto
                {
                    Id = p.Id,
                    Nome = p.Nome,
                    Descricao = p.Descricao,
                    Preco = p.Preco,
                    EstoqueMinimo = p.EstoqueMinimo,
                    CategoriaId = p.CategoriaId,
                    FornecedorId = p.FornecedorId,
                    Ativo = p.Ativo
                })
                .ToListAsync();

            return Ok(produtos);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Administrador,Gestor,Operador")]
        public async Task<ActionResult<ProdutoDto>>
            ObterPorId(int id)
        {
            var produto = await _db.Produtos
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new ProdutoDto
                {
                    Id = p.Id,
                    Nome = p.Nome,
                    Descricao = p.Descricao,
                    Preco = p.Preco,
                    EstoqueMinimo = p.EstoqueMinimo,
                    CategoriaId = p.CategoriaId,
                    FornecedorId = p.FornecedorId,
                    Ativo = p.Ativo
                })
                .FirstOrDefaultAsync();

            if (produto == null)
            {
                return NotFound(
                    "Produto não encontrado.");
            }

            return Ok(produto);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Gestor")]
        public async Task<ActionResult<ProdutoDto>> Criar(
            [FromBody] CriarProdutoDto dto)
        {
            if (dto.Preco <= 0)
            {
                return BadRequest(
                    "O preço deve ser maior que zero.");
            }

            if (dto.EstoqueMinimo < 0)
            {
                return BadRequest(
                    "O estoque mínimo não pode ser negativo.");
            }

            var categoriaExiste =
                await _db.Categorias.AnyAsync(c =>
                    c.Id == dto.CategoriaId &&
                    c.Ativa);

            if (!categoriaExiste)
            {
                return BadRequest(
                    "A categoria informada não existe ou está inativa.");
            }

            if (dto.FornecedorId.HasValue)
            {
                var fornecedorExiste =
                    await _db.Fornecedores.AnyAsync(f =>
                        f.Id == dto.FornecedorId.Value &&
                        f.Ativo);

                if (!fornecedorExiste)
                {
                    return BadRequest(
                        "O fornecedor informado não existe ou está inativo.");
                }
            }

            var produto = new Produto
            {
                Nome = dto.Nome.Trim(),
                Descricao = dto.Descricao.Trim(),
                Preco = dto.Preco,
                EstoqueMinimo = dto.EstoqueMinimo,
                CategoriaId = dto.CategoriaId,
                FornecedorId = dto.FornecedorId,
                Ativo = dto.Ativo
            };

            _db.Produtos.Add(produto);
            await _db.SaveChangesAsync();

            return CreatedAtAction(
                nameof(ObterPorId),
                new { id = produto.Id },
                ParaDto(produto));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador,Gestor")]
        public async Task<IActionResult> Atualizar(
            int id,
            [FromBody] AtualizarProdutoDto dto)
        {
            var produto = await _db.Produtos
                .FirstOrDefaultAsync(p => p.Id == id);

            if (produto == null)
            {
                return NotFound(
                    "Produto não encontrado.");
            }

            if (dto.Preco.HasValue &&
                dto.Preco.Value <= 0)
            {
                return BadRequest(
                    "O preço deve ser maior que zero.");
            }

            if (dto.EstoqueMinimo.HasValue &&
                dto.EstoqueMinimo.Value < 0)
            {
                return BadRequest(
                    "O estoque mínimo não pode ser negativo.");
            }

            if (dto.CategoriaId.HasValue)
            {
                var categoriaExiste =
                    await _db.Categorias.AnyAsync(c =>
                        c.Id == dto.CategoriaId.Value &&
                        c.Ativa);

                if (!categoriaExiste)
                {
                    return BadRequest(
                        "A categoria informada não existe ou está inativa.");
                }

                produto.CategoriaId =
                    dto.CategoriaId.Value;
            }

            if (dto.FornecedorId.HasValue)
            {
                var fornecedorExiste =
                    await _db.Fornecedores.AnyAsync(f =>
                        f.Id == dto.FornecedorId.Value &&
                        f.Ativo);

                if (!fornecedorExiste)
                {
                    return BadRequest(
                        "O fornecedor informado não existe ou está inativo.");
                }

                produto.FornecedorId =
                    dto.FornecedorId.Value;
            }

            if (!string.IsNullOrWhiteSpace(dto.Nome))
            {
                produto.Nome = dto.Nome.Trim();
            }

            if (dto.Descricao != null)
            {
                produto.Descricao =
                    dto.Descricao.Trim();
            }

            if (dto.Preco.HasValue)
            {
                produto.Preco = dto.Preco.Value;
            }

            if (dto.EstoqueMinimo.HasValue)
            {
                produto.EstoqueMinimo =
                    dto.EstoqueMinimo.Value;
            }

            if (dto.Ativo.HasValue)
            {
                produto.Ativo = dto.Ativo.Value;
            }

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Inativar(int id)
        {
            var produto = await _db.Produtos
                .FirstOrDefaultAsync(p => p.Id == id);

            if (produto == null)
            {
                return NotFound(
                    "Produto não encontrado.");
            }

            produto.Ativo = false;
            await _db.SaveChangesAsync();

            return NoContent();
        }

        private static ProdutoDto ParaDto(Produto produto)
        {
            return new ProdutoDto
            {
                Id = produto.Id,
                Nome = produto.Nome,
                Descricao = produto.Descricao,
                Preco = produto.Preco,
                EstoqueMinimo = produto.EstoqueMinimo,
                CategoriaId = produto.CategoriaId,
                FornecedorId = produto.FornecedorId,
                Ativo = produto.Ativo
            };
        }
    }
}
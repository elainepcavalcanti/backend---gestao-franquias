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
    public class FornecedoresController : ControllerBase
    {
        private readonly FranquiasDbContext _db;

        public FornecedoresController(
            FranquiasDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        [Authorize(Roles = "Administrador,Gestor,Operador")]
        public async Task<ActionResult<List<FornecedorDto>>>
            ObterTodos(
                [FromQuery] string? nome,
                [FromQuery] string? cnpj,
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

            if (tamanhoPagina < 1 ||
                tamanhoPagina > 100)
            {
                return BadRequest(
                    "O tamanho da página deve estar entre 1 e 100.");
            }

            var query = _db.Fornecedores
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(nome))
            {
                var termo = nome.Trim();

                query = query.Where(
                    f => f.Nome.Contains(termo));
            }

            if (!string.IsNullOrWhiteSpace(cnpj))
            {
                var termo = cnpj.Trim();

                query = query.Where(
                    f => f.Cnpj.Contains(termo));
            }

            if (ativo.HasValue)
            {
                query = query.Where(
                    f => f.Ativo == ativo.Value);
            }

            var total = await query.CountAsync();
            Response.Headers["X-Total-Count"] =
                total.ToString();

            switch (ordenarPor.Trim().ToLowerInvariant())
            {
                case "id":
                    query = decrescente
                        ? query.OrderByDescending(f => f.Id)
                        : query.OrderBy(f => f.Id);
                    break;

                case "cnpj":
                    query = decrescente
                        ? query.OrderByDescending(
                            f => f.Cnpj)
                        : query.OrderBy(f => f.Cnpj);
                    break;

                default:
                    query = decrescente
                        ? query.OrderByDescending(
                            f => f.Nome)
                        : query.OrderBy(f => f.Nome);
                    break;
            }

            var fornecedores = await query
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .Select(f => new FornecedorDto
                {
                    Id = f.Id,
                    Nome = f.Nome,
                    Cnpj = f.Cnpj,
                    Email = f.Email,
                    Telefone = f.Telefone,
                    Ativo = f.Ativo
                })
                .ToListAsync();

            return Ok(fornecedores);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Administrador,Gestor,Operador")]
        public async Task<ActionResult<FornecedorDto>>
            ObterPorId(int id)
        {
            var fornecedor = await _db.Fornecedores
                .AsNoTracking()
                .Where(f => f.Id == id)
                .Select(f => new FornecedorDto
                {
                    Id = f.Id,
                    Nome = f.Nome,
                    Cnpj = f.Cnpj,
                    Email = f.Email,
                    Telefone = f.Telefone,
                    Ativo = f.Ativo
                })
                .FirstOrDefaultAsync();

            if (fornecedor == null)
            {
                return NotFound(
                    "Fornecedor não encontrado.");
            }

            return Ok(fornecedor);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Gestor")]
        public async Task<ActionResult<FornecedorDto>> Criar(
            [FromBody] CriarFornecedorDto dto)
        {
            var cnpj = dto.Cnpj.Trim();

            var cnpjExistente =
                await _db.Fornecedores.AnyAsync(
                    f => f.Cnpj == cnpj);

            if (cnpjExistente)
            {
                return BadRequest(
                    "Já existe um fornecedor com este CNPJ.");
            }

            var fornecedor = new Fornecedor
            {
                Nome = dto.Nome.Trim(),
                Cnpj = cnpj,
                Email = dto.Email.Trim(),
                Telefone = dto.Telefone.Trim(),
                Ativo = true
            };

            _db.Fornecedores.Add(fornecedor);
            await _db.SaveChangesAsync();

            return CreatedAtAction(
                nameof(ObterPorId),
                new { id = fornecedor.Id },
                ParaDto(fornecedor));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador,Gestor")]
        public async Task<IActionResult> Atualizar(
            int id,
            [FromBody] AtualizarFornecedorDto dto)
        {
            var fornecedor = await _db.Fornecedores
                .FirstOrDefaultAsync(f => f.Id == id);

            if (fornecedor == null)
            {
                return NotFound(
                    "Fornecedor não encontrado.");
            }

            if (!string.IsNullOrWhiteSpace(dto.Cnpj))
            {
                var cnpj = dto.Cnpj.Trim();

                var cnpjExistente =
                    await _db.Fornecedores.AnyAsync(f =>
                        f.Id != id &&
                        f.Cnpj == cnpj);

                if (cnpjExistente)
                {
                    return BadRequest(
                        "Já existe um fornecedor com este CNPJ.");
                }

                fornecedor.Cnpj = cnpj;
            }

            if (!string.IsNullOrWhiteSpace(dto.Nome))
            {
                fornecedor.Nome = dto.Nome.Trim();
            }

            if (dto.Email != null)
            {
                fornecedor.Email =
                    dto.Email.Trim();
            }

            if (dto.Telefone != null)
            {
                fornecedor.Telefone =
                    dto.Telefone.Trim();
            }

            if (dto.Ativo.HasValue)
            {
                fornecedor.Ativo =
                    dto.Ativo.Value;
            }

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Inativar(int id)
        {
            var fornecedor = await _db.Fornecedores
                .FirstOrDefaultAsync(f => f.Id == id);

            if (fornecedor == null)
            {
                return NotFound(
                    "Fornecedor não encontrado.");
            }

            fornecedor.Ativo = false;
            await _db.SaveChangesAsync();

            return NoContent();
        }

        private static FornecedorDto ParaDto(
            Fornecedor fornecedor)
        {
            return new FornecedorDto
            {
                Id = fornecedor.Id,
                Nome = fornecedor.Nome,
                Cnpj = fornecedor.Cnpj,
                Email = fornecedor.Email,
                Telefone = fornecedor.Telefone,
                Ativo = fornecedor.Ativo
            };
        }
    }
}
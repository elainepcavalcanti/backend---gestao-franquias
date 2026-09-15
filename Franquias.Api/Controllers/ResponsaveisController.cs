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
    public class ResponsaveisController : ControllerBase
    {
        private readonly FranquiasDbContext _db;

        public ResponsaveisController(FranquiasDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        [Authorize(Roles = "Administrador,Gestor,Operador")]
        public async Task<ActionResult<List<ResponsavelDto>>> ObterTodos(
            [FromQuery] int? unidadeId,
            [FromQuery] string? nome,
            [FromQuery] string? cpf,
            [FromQuery] bool? ativo,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanhoPagina = 20,
            [FromQuery] string ordenarPor = "nome",
            [FromQuery] bool decrescente = false)
        {
            if (pagina < 1)
            {
                return BadRequest("A página deve ser maior ou igual a 1.");
            }

            if (tamanhoPagina < 1 || tamanhoPagina > 100)
            {
                return BadRequest(
                    "O tamanho da página deve estar entre 1 e 100.");
            }

            var query = _db.Responsaveis
                .AsNoTracking()
                .AsQueryable();

            if (unidadeId.HasValue)
            {
                query = query.Where(
                    r => r.UnidadeId == unidadeId.Value);
            }

            if (!string.IsNullOrWhiteSpace(nome))
            {
                query = query.Where(
                    r => r.Nome.Contains(nome.Trim()));
            }

            if (!string.IsNullOrWhiteSpace(cpf))
            {
                query = query.Where(
                    r => r.Cpf.Contains(cpf.Trim()));
            }

            if (ativo.HasValue)
            {
                query = query.Where(
                    r => r.Ativo == ativo.Value);
            }

            var total = await query.CountAsync();
            Response.Headers["X-Total-Count"] = total.ToString();

            switch (ordenarPor.Trim().ToLowerInvariant())
            {
                case "id":
                    query = decrescente
                        ? query.OrderByDescending(r => r.Id)
                        : query.OrderBy(r => r.Id);
                    break;

                case "cpf":
                    query = decrescente
                        ? query.OrderByDescending(r => r.Cpf)
                        : query.OrderBy(r => r.Cpf);
                    break;

                default:
                    query = decrescente
                        ? query.OrderByDescending(r => r.Nome)
                        : query.OrderBy(r => r.Nome);
                    break;
            }

            var responsaveis = await query
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .Select(r => new ResponsavelDto
                {
                    Id = r.Id,
                    UnidadeId = r.UnidadeId,
                    UnidadeNome = r.Unidade != null
                        ? r.Unidade.Nome
                        : string.Empty,
                    Nome = r.Nome,
                    Cpf = r.Cpf,
                    Telefone = r.Telefone,
                    Email = r.Email,
                    Ativo = r.Ativo
                })
                .ToListAsync();

            return Ok(responsaveis);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Administrador,Gestor,Operador")]
        public async Task<ActionResult<ResponsavelDto>> ObterPorId(int id)
        {
            var responsavel = await _db.Responsaveis
                .AsNoTracking()
                .Where(r => r.Id == id)
                .Select(r => new ResponsavelDto
                {
                    Id = r.Id,
                    UnidadeId = r.UnidadeId,
                    UnidadeNome = r.Unidade != null
                        ? r.Unidade.Nome
                        : string.Empty,
                    Nome = r.Nome,
                    Cpf = r.Cpf,
                    Telefone = r.Telefone,
                    Email = r.Email,
                    Ativo = r.Ativo
                })
                .FirstOrDefaultAsync();

            if (responsavel == null)
            {
                return NotFound("Responsável não encontrado.");
            }

            return Ok(responsavel);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Gestor")]
        public async Task<ActionResult<ResponsavelDto>> Criar(
            [FromBody] CriarResponsavelDto dto)
        {
            var unidade = await _db.Unidades
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == dto.UnidadeId);

            if (unidade == null)
            {
                return BadRequest("A unidade informada não existe.");
            }

            var cpf = dto.Cpf.Trim();

            var cpfExistente = await _db.Responsaveis.AnyAsync(
                r => r.UnidadeId == dto.UnidadeId &&
                     r.Cpf == cpf);

            if (cpfExistente)
            {
                return BadRequest(
                    "Já existe um responsável com este CPF nesta unidade.");
            }

            var responsavel = new Responsavel
            {
                UnidadeId = dto.UnidadeId,
                Nome = dto.Nome.Trim(),
                Cpf = cpf,
                Telefone = dto.Telefone.Trim(),
                Email = dto.Email.Trim(),
                Ativo = true
            };

            _db.Responsaveis.Add(responsavel);
            await _db.SaveChangesAsync();

            var resposta = new ResponsavelDto
            {
                Id = responsavel.Id,
                UnidadeId = responsavel.UnidadeId,
                UnidadeNome = unidade.Nome,
                Nome = responsavel.Nome,
                Cpf = responsavel.Cpf,
                Telefone = responsavel.Telefone,
                Email = responsavel.Email,
                Ativo = responsavel.Ativo
            };

            return CreatedAtAction(
                nameof(ObterPorId),
                new { id = responsavel.Id },
                resposta);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador,Gestor")]
        public async Task<IActionResult> Atualizar(
            int id,
            [FromBody] AtualizarResponsavelDto dto)
        {
            var responsavel = await _db.Responsaveis
                .FirstOrDefaultAsync(r => r.Id == id);

            if (responsavel == null)
            {
                return NotFound("Responsável não encontrado.");
            }

            if (!string.IsNullOrWhiteSpace(dto.Cpf))
            {
                var cpf = dto.Cpf.Trim();

                var cpfExistente = await _db.Responsaveis.AnyAsync(
                    r => r.Id != id &&
                         r.UnidadeId == responsavel.UnidadeId &&
                         r.Cpf == cpf);

                if (cpfExistente)
                {
                    return BadRequest(
                        "Já existe um responsável com este CPF nesta unidade.");
                }

                responsavel.Cpf = cpf;
            }

            if (!string.IsNullOrWhiteSpace(dto.Nome))
            {
                responsavel.Nome = dto.Nome.Trim();
            }

            if (dto.Telefone != null)
            {
                responsavel.Telefone = dto.Telefone.Trim();
            }

            if (dto.Email != null)
            {
                responsavel.Email = dto.Email.Trim();
            }

            if (dto.Ativo.HasValue)
            {
                responsavel.Ativo = dto.Ativo.Value;
            }

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador,Gestor")]
        public async Task<IActionResult> Inativar(int id)
        {
            var responsavel = await _db.Responsaveis
                .FirstOrDefaultAsync(r => r.Id == id);

            if (responsavel == null)
            {
                return NotFound("Responsável não encontrado.");
            }

            responsavel.Ativo = false;
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
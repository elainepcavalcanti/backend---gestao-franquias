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
    public class UnidadesController : ControllerBase
    {
        private readonly FranquiasDbContext _db;

        public UnidadesController(FranquiasDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        [Authorize(Roles = "Administrador,Gestor,Operador")]
        public async Task<ActionResult<List<UnidadeDto>>> ObterTodos(
            [FromQuery] int? franqueadoraId,
            [FromQuery] string? nome,
            [FromQuery] string? cidade,
            [FromQuery] string? cnpj,
            [FromQuery] string? responsavel,
            [FromQuery] bool? ativa,
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

            var query = _db.Unidades
                .AsNoTracking()
                .AsQueryable();

            if (franqueadoraId.HasValue)
            {
                query = query.Where(
                    u => u.FranqueadoraId ==
                         franqueadoraId.Value);
            }

            if (!string.IsNullOrWhiteSpace(nome))
            {
                var termo = nome.Trim();

                query = query.Where(
                    u => u.Nome.Contains(termo));
            }

            if (!string.IsNullOrWhiteSpace(cidade))
            {
                var termo = cidade.Trim();

                query = query.Where(
                    u => u.Cidade.Contains(termo));
            }

            if (!string.IsNullOrWhiteSpace(cnpj))
            {
                var termo = cnpj.Trim();

                query = query.Where(
                    u => u.Cnpj.Contains(termo));
            }

            if (!string.IsNullOrWhiteSpace(responsavel))
            {
                var termo = responsavel.Trim();

                query = query.Where(u =>
                    _db.Responsaveis.Any(r =>
                        r.UnidadeId == u.Id &&
                        r.Ativo &&
                        r.Nome.Contains(termo)));
            }

            if (ativa.HasValue)
            {
                query = query.Where(
                    u => u.Ativa == ativa.Value);
            }

            var total = await query.CountAsync();
            Response.Headers["X-Total-Count"] =
                total.ToString();

            switch (ordenarPor.Trim().ToLowerInvariant())
            {
                case "id":
                    query = decrescente
                        ? query.OrderByDescending(u => u.Id)
                        : query.OrderBy(u => u.Id);
                    break;

                case "cidade":
                    query = decrescente
                        ? query.OrderByDescending(u => u.Cidade)
                        : query.OrderBy(u => u.Cidade);
                    break;

                case "cnpj":
                    query = decrescente
                        ? query.OrderByDescending(u => u.Cnpj)
                        : query.OrderBy(u => u.Cnpj);
                    break;

                case "datainicio":
                    query = decrescente
                        ? query.OrderByDescending(
                            u => u.DataInicio)
                        : query.OrderBy(u => u.DataInicio);
                    break;

                default:
                    query = decrescente
                        ? query.OrderByDescending(u => u.Nome)
                        : query.OrderBy(u => u.Nome);
                    break;
            }

            var unidades = await query
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .Select(u => new UnidadeDto
                {
                    Id = u.Id,
                    Nome = u.Nome,
                    Cnpj = u.Cnpj,
                    Endereco = u.Endereco,
                    Cidade = u.Cidade,
                    Estado = u.Estado,
                    Telefone = u.Telefone,
                    PercentualRoyalty =
                        u.PercentualRoyalty,
                    Ativa = u.Ativa,
                    DataInicio = u.DataInicio,
                    FranqueadoraId =
                        u.FranqueadoraId
                })
                .ToListAsync();

            return Ok(unidades);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Administrador,Gestor,Operador")]
        public async Task<ActionResult<UnidadeDto>>
            ObterPorId(int id)
        {
            var unidade = await _db.Unidades
                .AsNoTracking()
                .Where(u => u.Id == id)
                .Select(u => new UnidadeDto
                {
                    Id = u.Id,
                    Nome = u.Nome,
                    Cnpj = u.Cnpj,
                    Endereco = u.Endereco,
                    Cidade = u.Cidade,
                    Estado = u.Estado,
                    Telefone = u.Telefone,
                    PercentualRoyalty =
                        u.PercentualRoyalty,
                    Ativa = u.Ativa,
                    DataInicio = u.DataInicio,
                    FranqueadoraId =
                        u.FranqueadoraId
                })
                .FirstOrDefaultAsync();

            if (unidade == null)
            {
                return NotFound(
                    "Unidade não encontrada.");
            }

            return Ok(unidade);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Gestor")]
        public async Task<ActionResult<UnidadeDto>> Criar(
            [FromBody] CriarUnidadeDto dto)
        {
            var franqueadoraExiste =
                await _db.Franqueadoras.AnyAsync(
                    f => f.Id == dto.FranqueadoraId);

            if (!franqueadoraExiste)
            {
                return BadRequest(
                    "Franqueadora informada não existe.");
            }

            var cnpj = dto.Cnpj.Trim();

            if (await _db.Unidades.AnyAsync(
                u => u.Cnpj == cnpj))
            {
                return BadRequest(
                    "Já existe uma unidade com este CNPJ.");
            }

            var unidade = new Unidade
            {
                Nome = dto.Nome.Trim(),
                Cnpj = cnpj,
                Endereco = dto.Endereco.Trim(),
                Cidade = dto.Cidade.Trim(),
                Estado = dto.Estado.Trim(),
                Telefone = dto.Telefone.Trim(),
                PercentualRoyalty =
                    dto.PercentualRoyalty,
                Ativa = true,
                DataInicio = DateTime.UtcNow,
                FranqueadoraId =
                    dto.FranqueadoraId
            };

            _db.Unidades.Add(unidade);
            await _db.SaveChangesAsync();

            return CreatedAtAction(
                nameof(ObterPorId),
                new { id = unidade.Id },
                ParaDto(unidade));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador,Gestor")]
        public async Task<IActionResult> Atualizar(
            int id,
            [FromBody] AtualizarUnidadeDto dto)
        {
            var unidade = await _db.Unidades
                .FirstOrDefaultAsync(u => u.Id == id);

            if (unidade == null)
            {
                return NotFound(
                    "Unidade não encontrada.");
            }

            if (!string.IsNullOrWhiteSpace(dto.Cnpj))
            {
                var cnpj = dto.Cnpj.Trim();

                var cnpjExistente =
                    await _db.Unidades.AnyAsync(u =>
                        u.Id != id &&
                        u.Cnpj == cnpj);

                if (cnpjExistente)
                {
                    return BadRequest(
                        "Já existe uma unidade com este CNPJ.");
                }

                unidade.Cnpj = cnpj;
            }

            if (!string.IsNullOrWhiteSpace(dto.Nome))
            {
                unidade.Nome = dto.Nome.Trim();
            }

            if (!string.IsNullOrWhiteSpace(dto.Endereco))
            {
                unidade.Endereco =
                    dto.Endereco.Trim();
            }

            if (!string.IsNullOrWhiteSpace(dto.Cidade))
            {
                unidade.Cidade = dto.Cidade.Trim();
            }

            if (!string.IsNullOrWhiteSpace(dto.Estado))
            {
                unidade.Estado = dto.Estado.Trim();
            }

            if (dto.Telefone != null)
            {
                unidade.Telefone =
                    dto.Telefone.Trim();
            }

            if (dto.PercentualRoyalty.HasValue)
            {
                if (dto.PercentualRoyalty.Value < 0 ||
                    dto.PercentualRoyalty.Value > 100)
                {
                    return BadRequest(
                        "O percentual de royalty deve estar entre 0 e 100.");
                }

                unidade.PercentualRoyalty =
                    dto.PercentualRoyalty.Value;
            }

            if (dto.Ativa.HasValue)
            {
                unidade.Ativa = dto.Ativa.Value;
            }

            if (dto.DataInicio.HasValue)
            {
                unidade.DataInicio =
                    dto.DataInicio.Value;
            }

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Inativar(int id)
        {
            var unidade = await _db.Unidades
                .FirstOrDefaultAsync(u => u.Id == id);

            if (unidade == null)
            {
                return NotFound(
                    "Unidade não encontrada.");
            }

            unidade.Ativa = false;
            await _db.SaveChangesAsync();

            return NoContent();
        }

        private static UnidadeDto ParaDto(Unidade unidade)
        {
            return new UnidadeDto
            {
                Id = unidade.Id,
                Nome = unidade.Nome,
                Cnpj = unidade.Cnpj,
                Endereco = unidade.Endereco,
                Cidade = unidade.Cidade,
                Estado = unidade.Estado,
                Telefone = unidade.Telefone,
                PercentualRoyalty =
                    unidade.PercentualRoyalty,
                Ativa = unidade.Ativa,
                DataInicio = unidade.DataInicio,
                FranqueadoraId =
                    unidade.FranqueadoraId
            };
        }
    }
}
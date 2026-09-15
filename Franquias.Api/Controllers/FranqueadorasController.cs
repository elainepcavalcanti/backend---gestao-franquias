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
    [Route("api/franqueadoras")]
    public class FranqueadorasController : ControllerBase
    {
        private readonly FranquiasDbContext _db;

        public FranqueadorasController(FranquiasDbContext db)
        {
            _db = db;
        }

        [HttpPost("validar")]
        [ProducesResponseType(typeof(CriarFranqueadoraDto), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        public ActionResult<CriarFranqueadoraDto> Validar(
            [FromBody] CriarFranqueadoraDto dados)
        {
            return Ok(dados);
        }

        [HttpGet("test-connection")]
        public ActionResult<bool> TestConnection()
        {
            try
            {
                return Ok(_db.Database.CanConnect());
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Administrador,Gestor")]
        public async Task<ActionResult<List<FranqueadoraDto>>> ObterTodos(
            [FromQuery] string? nome,
            [FromQuery] string? cnpj,
            [FromQuery] bool? ativa,
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
                return BadRequest("O tamanho da página deve estar entre 1 e 100.");
            }

            var query = _db.Franqueadoras
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(nome))
            {
                var nomePesquisa = nome.Trim();

                query = query.Where(f =>
                    f.NomeFantasia.Contains(nomePesquisa) ||
                    f.RazaoSocial.Contains(nomePesquisa));
            }

            if (!string.IsNullOrWhiteSpace(cnpj))
            {
                var cnpjPesquisa = cnpj.Trim();
                query = query.Where(f => f.Cnpj.Contains(cnpjPesquisa));
            }

            if (ativa.HasValue)
            {
                query = query.Where(f => f.Ativa == ativa.Value);
            }

            var total = await query.CountAsync();
            Response.Headers["X-Total-Count"] = total.ToString();

            query = ordenarPor.Trim().ToLowerInvariant() switch
            {
                "razaosocial" => decrescente
                    ? query.OrderByDescending(f => f.RazaoSocial)
                    : query.OrderBy(f => f.RazaoSocial),

                "cnpj" => decrescente
                    ? query.OrderByDescending(f => f.Cnpj)
                    : query.OrderBy(f => f.Cnpj),

                "datacadastro" => decrescente
                    ? query.OrderByDescending(f => f.DataCadastro)
                    : query.OrderBy(f => f.DataCadastro),

                "id" => decrescente
                    ? query.OrderByDescending(f => f.Id)
                    : query.OrderBy(f => f.Id),

                _ => decrescente
                    ? query.OrderByDescending(f => f.NomeFantasia)
                    : query.OrderBy(f => f.NomeFantasia)
            };

            var franqueadoras = await query
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .Select(f => new FranqueadoraDto
                {
                    Id = f.Id,
                    Nome = f.NomeFantasia,
                    NomeFantasia = f.NomeFantasia,
                    RazaoSocial = f.RazaoSocial,
                    Cnpj = f.Cnpj,
                    Email = f.Email,
                    Ativa = f.Ativa,
                    DataCadastro = f.DataCadastro
                })
                .ToListAsync();

            return Ok(franqueadoras);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Administrador,Gestor")]
        public async Task<ActionResult<FranqueadoraDto>> ObterPorId(int id)
        {
            var franqueadora = await _db.Franqueadoras
                .AsNoTracking()
                .Where(f => f.Id == id)
                .Select(f => new FranqueadoraDto
                {
                    Id = f.Id,
                    Nome = f.NomeFantasia,
                    NomeFantasia = f.NomeFantasia,
                    RazaoSocial = f.RazaoSocial,
                    Cnpj = f.Cnpj,
                    Email = f.Email,
                    Ativa = f.Ativa,
                    DataCadastro = f.DataCadastro
                })
                .FirstOrDefaultAsync();

            if (franqueadora == null)
            {
                return NotFound("Franqueadora não encontrada.");
            }

            return Ok(franqueadora);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<FranqueadoraDto>> Criar(
            [FromBody] CriarFranqueadoraDto dto)
        {
            var nome = dto.Nome.Trim();
            var nomeFantasia = string.IsNullOrWhiteSpace(dto.NomeFantasia)
                ? nome
                : dto.NomeFantasia.Trim();

            var razaoSocial = string.IsNullOrWhiteSpace(dto.RazaoSocial)
                ? nome
                : dto.RazaoSocial.Trim();

            var cnpj = dto.Cnpj.Trim();
            var email = dto.Email.Trim().ToLowerInvariant();

            if (await _db.Franqueadoras.AnyAsync(f => f.Cnpj == cnpj))
            {
                return BadRequest("Já existe uma franqueadora com este CNPJ.");
            }

            var franqueadora = new Franqueadora
            {
                NomeFantasia = nomeFantasia,
                RazaoSocial = razaoSocial,
                Cnpj = cnpj,
                Email = email,
                Ativa = true,
                DataCadastro = DateTime.UtcNow
            };

            _db.Franqueadoras.Add(franqueadora);
            await _db.SaveChangesAsync();

            var resposta = new FranqueadoraDto
            {
                Id = franqueadora.Id,
                Nome = franqueadora.NomeFantasia,
                NomeFantasia = franqueadora.NomeFantasia,
                RazaoSocial = franqueadora.RazaoSocial,
                Cnpj = franqueadora.Cnpj,
                Email = franqueadora.Email,
                Ativa = franqueadora.Ativa,
                DataCadastro = franqueadora.DataCadastro
            };

            return CreatedAtAction(
                nameof(ObterPorId),
                new { id = franqueadora.Id },
                resposta);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Atualizar(
            int id,
            [FromBody] AtualizarFranqueadoraDto dto)
        {
            var franqueadora = await _db.Franqueadoras
                .FirstOrDefaultAsync(f => f.Id == id);

            if (franqueadora == null)
            {
                return NotFound("Franqueadora não encontrada.");
            }

            if (!string.IsNullOrWhiteSpace(dto.Cnpj))
            {
                var novoCnpj = dto.Cnpj.Trim();

                var cnpjEmUso = await _db.Franqueadoras
                    .AnyAsync(f => f.Id != id && f.Cnpj == novoCnpj);

                if (cnpjEmUso)
                {
                    return BadRequest("Já existe outra franqueadora com este CNPJ.");
                }

                franqueadora.Cnpj = novoCnpj;
            }

            if (!string.IsNullOrWhiteSpace(dto.Nome))
            {
                franqueadora.NomeFantasia = dto.Nome.Trim();
            }

            if (!string.IsNullOrWhiteSpace(dto.NomeFantasia))
            {
                franqueadora.NomeFantasia = dto.NomeFantasia.Trim();
            }

            if (!string.IsNullOrWhiteSpace(dto.RazaoSocial))
            {
                franqueadora.RazaoSocial = dto.RazaoSocial.Trim();
            }

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                franqueadora.Email = dto.Email.Trim().ToLowerInvariant();
            }

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Inativar(int id)
        {
            var franqueadora = await _db.Franqueadoras
                .FirstOrDefaultAsync(f => f.Id == id);

            if (franqueadora == null)
            {
                return NotFound("Franqueadora não encontrada.");
            }

            if (!franqueadora.Ativa)
            {
                return NoContent();
            }

            franqueadora.Ativa = false;

            await _db.Unidades
                .Where(u => u.FranqueadoraId == id && u.Ativa)
                .ExecuteUpdateAsync(alteracoes => alteracoes
                    .SetProperty(u => u.Ativa, false));

            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
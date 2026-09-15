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
    public class ChamadosController : ControllerBase
    {
        private readonly FranquiasDbContext _db;

        public ChamadosController(FranquiasDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        [Authorize(Roles = "Administrador,Gestor,Operador")]
        public async Task<ActionResult<List<ChamadoDto>>> ObterTodos(
            [FromQuery] int? unidadeId,
            [FromQuery] StatusChamado? status)
        {
            var usuario = await ObterUsuarioAutenticado();

            if (usuario == null)
            {
                return Unauthorized("Usuário não encontrado ou inativo.");
            }

            var query = _db.Chamados
                .AsNoTracking()
                .Include(c => c.Unidade)
                .Include(c => c.Usuario)
                .AsQueryable();

            if (usuario.Perfil == PerfilUsuario.Operador)
            {
                if (!usuario.UnidadeId.HasValue)
                {
                    return Forbid();
                }

                query = query.Where(c =>
                    c.UnidadeId == usuario.UnidadeId.Value);
            }
            else if (unidadeId.HasValue)
            {
                query = query.Where(c =>
                    c.UnidadeId == unidadeId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(c => c.Status == status.Value);
            }

            var chamados = await query
                .OrderByDescending(c => c.DataAbertura)
                .ToListAsync();

            return Ok(chamados.Select(ParaDto).ToList());
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Administrador,Gestor,Operador")]
        public async Task<ActionResult<ChamadoDto>> ObterPorId(int id)
        {
            var usuario = await ObterUsuarioAutenticado();

            if (usuario == null)
            {
                return Unauthorized("Usuário não encontrado ou inativo.");
            }

            var query = _db.Chamados
                .AsNoTracking()
                .Include(c => c.Unidade)
                .Include(c => c.Usuario)
                .Where(c => c.Id == id);

            if (usuario.Perfil == PerfilUsuario.Operador)
            {
                if (!usuario.UnidadeId.HasValue)
                {
                    return Forbid();
                }

                query = query.Where(c =>
                    c.UnidadeId == usuario.UnidadeId.Value);
            }

            var chamado = await query.FirstOrDefaultAsync();

            if (chamado == null)
            {
                return NotFound("Chamado não encontrado.");
            }

            return Ok(ParaDto(chamado));
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Gestor,Operador")]
        public async Task<ActionResult<ChamadoDto>> Criar(
            [FromBody] CriarChamadoDto dto)
        {
            var usuario = await ObterUsuarioAutenticado();

            if (usuario == null)
            {
                return Unauthorized("Usuário não encontrado ou inativo.");
            }

            var unidade = await _db.Unidades
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == dto.UnidadeId);

            if (unidade == null)
            {
                return BadRequest("Unidade informada não existe.");
            }

            if (!unidade.Ativa)
            {
                return BadRequest(
                    "Não é possível abrir chamado para uma unidade inativa.");
            }

            if (usuario.Perfil == PerfilUsuario.Operador &&
                usuario.UnidadeId != dto.UnidadeId)
            {
                return Forbid();
            }

            var chamado = new Chamado
            {
                Titulo = dto.Titulo.Trim(),
                Descricao = dto.Descricao.Trim(),
                Categoria = dto.Categoria.Trim(),
                Prioridade = dto.Prioridade,
                Status = StatusChamado.Aberto,
                UnidadeId = dto.UnidadeId,
                UsuarioId = usuario.Id,
                DataAbertura = DateTime.UtcNow
            };

            _db.Chamados.Add(chamado);
            await _db.SaveChangesAsync();

            chamado.Unidade = unidade;
            chamado.Usuario = usuario;

            return CreatedAtAction(
                nameof(ObterPorId),
                new { id = chamado.Id },
                ParaDto(chamado));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador,Gestor,Operador")]
        public async Task<IActionResult> Atualizar(
            int id,
            [FromBody] AtualizarChamadoDto dto)
        {
            var usuario = await ObterUsuarioAutenticado();

            if (usuario == null)
            {
                return Unauthorized("Usuário não encontrado ou inativo.");
            }

            var chamado = await _db.Chamados
                .FirstOrDefaultAsync(c => c.Id == id);

            if (chamado == null)
            {
                return NotFound("Chamado não encontrado.");
            }

            if (usuario.Perfil == PerfilUsuario.Operador &&
                usuario.UnidadeId != chamado.UnidadeId)
            {
                return Forbid();
            }

            if (!string.IsNullOrWhiteSpace(dto.Titulo))
            {
                chamado.Titulo = dto.Titulo.Trim();
            }

            if (!string.IsNullOrWhiteSpace(dto.Descricao))
            {
                chamado.Descricao = dto.Descricao.Trim();
            }

            if (!string.IsNullOrWhiteSpace(dto.Categoria))
            {
                chamado.Categoria = dto.Categoria.Trim();
            }

            if (dto.Prioridade.HasValue)
            {
                chamado.Prioridade = dto.Prioridade.Value;
            }

            if (dto.Status.HasValue)
            {
                chamado.Status = dto.Status.Value;

                if (dto.Status == StatusChamado.Resolvido ||
                    dto.Status == StatusChamado.Fechado)
                {
                    chamado.DataFechamento ??= DateTime.UtcNow;
                }
                else
                {
                    chamado.DataFechamento = null;
                }
            }

            await _db.SaveChangesAsync();
            return NoContent();
        }

        private async Task<Usuario?> ObterUsuarioAutenticado()
        {
            var idDoToken =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                User.FindFirst("sub")?.Value;

            if (!int.TryParse(idDoToken, out var usuarioId))
            {
                return null;
            }

            return await _db.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u =>
                    u.Id == usuarioId && u.Ativo);
        }

        private static ChamadoDto ParaDto(Chamado chamado)
        {
            return new ChamadoDto
            {
                Id = chamado.Id,
                Titulo = chamado.Titulo,
                Descricao = chamado.Descricao,
                Categoria = chamado.Categoria,
                Prioridade = chamado.Prioridade,
                Status = chamado.Status,
                UnidadeId = chamado.UnidadeId,
                UnidadeNome = chamado.Unidade?.Nome ?? string.Empty,
                UsuarioId = chamado.UsuarioId,
                UsuarioNome = chamado.Usuario?.Nome ?? string.Empty,
                DataAbertura = chamado.DataAbertura,
                DataFechamento = chamado.DataFechamento
            };
        }
    }
}
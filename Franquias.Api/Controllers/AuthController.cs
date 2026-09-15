using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Franquias.Api.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Franquias.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly FranquiasDbContext _db;
        private readonly IConfiguration _configuration;

        public AuthController(
            FranquiasDbContext db,
            IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponseDto>> Login(
            [FromBody] AutenticarUsuarioDto dto)
        {
            var email = dto.Email.Trim();

            var usuario = await _db.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.Email == email &&
                    u.Ativo);

            if (usuario == null ||
                !AuthExtensions.VerifyPassword(
                    dto.Senha,
                    usuario.SenhaHash))
            {
                return Unauthorized("Credenciais inválidas.");
            }

            var token = AuthExtensions.GenerateJwtToken(
                usuario,
                _configuration);

            return Ok(new LoginResponseDto
            {
                Token = token,
                Usuario = ParaResumo(usuario)
            });
        }

        [HttpPost("primeiro-administrador")]
        [AllowAnonymous]
        public async Task<ActionResult<UsuarioResumoDto>>
            CriarPrimeiroAdministrador(
                [FromBody] CriarAdministradorInicialDto dto,
                [FromServices] IWebHostEnvironment ambiente)
        {
            var habilitado = _configuration.GetValue<bool>(
                "Bootstrap:PermitirAdministradorInicial",
                false);

            var enderecoCliente =
                HttpContext.Connection.RemoteIpAddress;

            if (!ambiente.IsDevelopment() ||
                !habilitado ||
                enderecoCliente == null ||
                !System.Net.IPAddress.IsLoopback(enderecoCliente))
            {
                return NotFound();
            }

            var existeAdministrador = await _db.Usuarios
                .AnyAsync(u =>
                    u.Perfil == PerfilUsuario.Administrador);

            if (existeAdministrador)
            {
                return Conflict(
                    "O administrador inicial já foi criado.");
            }

            var email = dto.Email.Trim();

            var emailExistente = await _db.Usuarios
                .AnyAsync(u => u.Email == email);

            if (emailExistente)
            {
                return BadRequest(
                    "Já existe usuário com este e-mail.");
            }

            var usuario = new Usuario
            {
                Nome = dto.Nome.Trim(),
                Email = email,
                SenhaHash =
                    AuthExtensions.HashPassword(dto.Senha),
                Perfil = PerfilUsuario.Administrador,
                UnidadeId = null,
                Ativo = true
            };

            _db.Usuarios.Add(usuario);
            await _db.SaveChangesAsync();

            return Ok(ParaResumo(usuario));
        }

        [HttpPost("usuarios")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<UsuarioResumoDto>> CriarUsuario(
            [FromBody] CriarUsuarioDto dto)
        {
            var email = dto.Email.Trim();

            var usuarioExistente = await _db.Usuarios
                .AnyAsync(u => u.Email == email);

            if (usuarioExistente)
            {
                return BadRequest(
                    "Já existe usuário com este e-mail.");
            }

            if (dto.UnidadeId.HasValue)
            {
                var unidadeExiste = await _db.Unidades
                    .AnyAsync(u =>
                        u.Id == dto.UnidadeId.Value);

                if (!unidadeExiste)
                {
                    return BadRequest(
                        "Unidade informada não existe.");
                }
            }

            var usuario = new Usuario
            {
                Nome = dto.Nome.Trim(),
                Email = email,
                SenhaHash =
                    AuthExtensions.HashPassword(dto.Senha),
                Perfil = dto.Perfil,
                UnidadeId = dto.UnidadeId,
                Ativo = true
            };

            _db.Usuarios.Add(usuario);
            await _db.SaveChangesAsync();

            return Ok(ParaResumo(usuario));
        }

        [HttpGet("usuarios")]
        [Authorize(Roles = "Administrador,Gestor")]
        public async Task<ActionResult<List<UsuarioResumoDto>>>
            ObterUsuarios([FromQuery] bool? ativo)
        {
            var query = _db.Usuarios
                .AsNoTracking()
                .AsQueryable();

            if (ativo.HasValue)
            {
                query = query.Where(
                    u => u.Ativo == ativo.Value);
            }

            var usuarios = await query
                .OrderBy(u => u.Nome)
                .Select(u => new UsuarioResumoDto
                {
                    Id = u.Id,
                    Nome = u.Nome,
                    Email = u.Email,
                    Perfil = u.Perfil.ToString(),
                    UnidadeId = u.UnidadeId,
                    Ativo = u.Ativo
                })
                .ToListAsync();

            return Ok(usuarios);
        }

        [HttpPatch("usuarios/{id}/status")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<UsuarioResumoDto>>
            AlterarStatusUsuario(
                int id,
                [FromBody] AtualizarStatusUsuarioDto dto)
        {
            var usuario = await _db.Usuarios
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
            {
                return NotFound("Usuário não encontrado.");
            }

            var identificadorAtual =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!dto.Ativo &&
                int.TryParse(
                    identificadorAtual,
                    out var usuarioAtualId) &&
                usuarioAtualId == id)
            {
                return BadRequest(
                    "Não é possível inativar o próprio usuário.");
            }

            if (!dto.Ativo &&
                usuario.Perfil == PerfilUsuario.Administrador)
            {
                var existeOutroAdministradorAtivo =
                    await _db.Usuarios.AnyAsync(u =>
                        u.Id != id &&
                        u.Perfil ==
                            PerfilUsuario.Administrador &&
                        u.Ativo);

                if (!existeOutroAdministradorAtivo)
                {
                    return BadRequest(
                        "Não é possível inativar o único administrador ativo.");
                }
            }

            usuario.Ativo = dto.Ativo;
            await _db.SaveChangesAsync();

            return Ok(ParaResumo(usuario));
        }

        private static UsuarioResumoDto ParaResumo(
            Usuario usuario)
        {
            return new UsuarioResumoDto
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                Perfil = usuario.Perfil.ToString(),
                UnidadeId = usuario.UnidadeId,
                Ativo = usuario.Ativo
            };
        }
    }
}
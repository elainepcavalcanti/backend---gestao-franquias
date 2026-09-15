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
    public class CategoriasController : ControllerBase
    {
        private readonly FranquiasDbContext _db;

        public CategoriasController(FranquiasDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        [Authorize(Roles = "Administrador,Gestor,Operador")]
        public async Task<ActionResult<List<Categoria>>> ObterTodos([FromQuery] int? franqueadoraId)
        {
            var query = _db.Categorias.AsQueryable();

            return Ok(await query.ToListAsync());
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Administrador,Gestor,Operador")]
        public async Task<ActionResult<Categoria>> ObterPorId(int id)
        {
            var categoria = await _db.Categorias.FindAsync(id);
            if (categoria == null)
            {
                return NotFound();
            }

            return Ok(categoria);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Gestor")]
        public async Task<ActionResult<Categoria>> Criar([FromBody] Categoria categoria)
        {
            _db.Categorias.Add(categoria);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(ObterPorId), new { id = categoria.Id }, categoria);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador,Gestor")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] Categoria categoria)
        {
            var categoriaExistente = await _db.Categorias.FindAsync(id);
            if (categoriaExistente == null)
            {
                return NotFound();
            }

            categoriaExistente.Nome = categoria.Nome;
            categoriaExistente.Ativa = categoria.Ativa;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Deletar(int id)
        {
            var categoria = await _db.Categorias.FindAsync(id);
            if (categoria == null)
            {
                return NotFound();
            }

            _db.Categorias.Remove(categoria);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}

using EscolaTI.Data;
using EscolaTI.Dtos;
using EscolaTI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EscolaTI.Controllers
{
    [ApiController]
    [Route("api/cursos")]
    public class CursosController : ControllerBase
    {
        private readonly EscolaContext _context;

        public CursosController(EscolaContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IEnumerable<Curso>> Listar()
        {
            return await _context.Cursos.AsNoTracking().OrderBy(c => c.Id).ToListAsync();
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Curso>> Obter(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            return curso is null ? NotFound() : curso;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Curso>> Criar(CursoRequest request)
        {
            var curso = new Curso { Nome = request.Nome.Trim(), DataInicio = request.DataInicio!.Value };
            _context.Cursos.Add(curso);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Obter), new { id = curso.Id }, curso);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Curso>> Atualizar(int id, CursoRequest request)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso is null)
            {
                return NotFound();
            }

            curso.Nome = request.Nome.Trim();
            curso.DataInicio = request.DataInicio!.Value;
            await _context.SaveChangesAsync();
            return curso;
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Excluir(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso is null)
            {
                return NotFound();
            }

            if (await _context.Inscricoes.AnyAsync(i => i.CursoId == id))
            {
                return Conflict(new ProblemDetails { Title = "O curso possui inscrições e não pode ser excluído.", Status = StatusCodes.Status409Conflict });
            }

            _context.Cursos.Remove(curso);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}

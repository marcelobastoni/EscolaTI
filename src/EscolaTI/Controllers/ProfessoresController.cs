using EscolaTI.Data;
using EscolaTI.Dtos;
using EscolaTI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EscolaTI.Controllers
{
    [ApiController]
    [Route("api/professores")]
    public class ProfessoresController : ControllerBase
    {
        private readonly EscolaContext _context;

        public ProfessoresController(EscolaContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IEnumerable<Professor>> Listar()
        {
            return await _context.Professores.AsNoTracking().OrderBy(p => p.Id).ToListAsync();
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Professor>> Obter(int id)
        {
            var professor = await _context.Professores.FindAsync(id);
            return professor is null ? NotFound() : professor;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<Professor>> Criar(ProfessorRequest request)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            if (await _context.Professores.AnyAsync(p => p.Email == email))
            {
                return Conflict(new ProblemDetails { Title = "Já existe um professor com este e-mail.", Status = StatusCodes.Status409Conflict });
            }

            var professor = new Professor { Nome = request.Nome.Trim(), Email = email, Titulacao = request.Titulacao.Trim() };
            _context.Professores.Add(professor);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Obter), new { id = professor.Id }, professor);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<Professor>> Atualizar(int id, ProfessorRequest request)
        {
            var professor = await _context.Professores.FindAsync(id);
            if (professor is null)
            {
                return NotFound();
            }

            var email = request.Email.Trim().ToLowerInvariant();
            if (await _context.Professores.AnyAsync(p => p.Email == email && p.Id != id))
            {
                return Conflict(new ProblemDetails { Title = "Já existe um professor com este e-mail.", Status = StatusCodes.Status409Conflict });
            }

            professor.Nome = request.Nome.Trim();
            professor.Email = email;
            professor.Titulacao = request.Titulacao.Trim();
            await _context.SaveChangesAsync();
            return professor;
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Excluir(int id)
        {
            var professor = await _context.Professores.FindAsync(id);
            if (professor is null)
            {
                return NotFound();
            }

            _context.Professores.Remove(professor);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}

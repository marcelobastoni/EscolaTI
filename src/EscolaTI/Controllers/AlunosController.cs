using EscolaTI.Data;
using EscolaTI.Dtos;
using EscolaTI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EscolaTI.Controllers
{
    [ApiController]
    [Route("api/alunos")]
    public class AlunosController : ControllerBase
    {
        private readonly EscolaContext _context;

        public AlunosController(EscolaContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IEnumerable<Aluno>> Listar()
        {
            return await _context.Alunos.AsNoTracking().OrderBy(a => a.Id).ToListAsync();
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Aluno>> Obter(int id)
        {
            var aluno = await _context.Alunos.FindAsync(id);
            return aluno is null ? NotFound() : aluno;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<Aluno>> Criar(AlunoRequest request)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            if (await _context.Alunos.AnyAsync(a => a.Email == email))
            {
                return Conflict(new ProblemDetails { Title = "Já existe um aluno com este e-mail.", Status = StatusCodes.Status409Conflict });
            }

            var aluno = new Aluno { Nome = request.Nome.Trim(), Email = email, Ativo = request.Ativo };
            _context.Alunos.Add(aluno);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Obter), new { id = aluno.Id }, aluno);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<Aluno>> Atualizar(int id, AlunoRequest request)
        {
            var aluno = await _context.Alunos.FindAsync(id);
            if (aluno is null)
            {
                return NotFound();
            }

            var email = request.Email.Trim().ToLowerInvariant();
            if (await _context.Alunos.AnyAsync(a => a.Email == email && a.Id != id))
            {
                return Conflict(new ProblemDetails { Title = "Já existe um aluno com este e-mail.", Status = StatusCodes.Status409Conflict });
            }

            aluno.Nome = request.Nome.Trim();
            aluno.Email = email;
            aluno.Ativo = request.Ativo;
            await _context.SaveChangesAsync();
            return aluno;
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Excluir(int id)
        {
            var aluno = await _context.Alunos.FindAsync(id);
            if (aluno is null)
            {
                return NotFound();
            }

            if (await _context.Inscricoes.AnyAsync(i => i.AlunoId == id))
            {
                return Conflict(new ProblemDetails { Title = "O aluno possui inscrições e não pode ser excluído.", Status = StatusCodes.Status409Conflict });
            }

            _context.Alunos.Remove(aluno);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}

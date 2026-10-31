using EscolaTI.Data;
using EscolaTI.Dtos;
using EscolaTI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EscolaTI.Controllers
{
    // Sem PUT: a inscrição é identificada pelo par aluno/curso e não tem outros campos.
    [ApiController]
    [Route("api/inscricoes")]
    public class InscricoesController : ControllerBase
    {
        private readonly EscolaContext _context;

        public InscricoesController(EscolaContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IEnumerable<Inscricao>> Listar()
        {
            return await _context.Inscricoes.AsNoTracking().OrderBy(i => i.AlunoId).ThenBy(i => i.CursoId).ToListAsync();
        }

        [HttpGet("{alunoId:int}/{cursoId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Inscricao>> Obter(int alunoId, int cursoId)
        {
            var inscricao = await _context.Inscricoes.FindAsync(alunoId, cursoId);
            return inscricao is null ? NotFound() : inscricao;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<Inscricao>> Criar(InscricaoRequest request)
        {
            var alunoId = request.AlunoId!.Value;
            var cursoId = request.CursoId!.Value;

            var aluno = await _context.Alunos.FindAsync(alunoId);
            if (aluno is null)
            {
                return BadRequest(new ProblemDetails { Title = "Aluno não encontrado.", Status = StatusCodes.Status400BadRequest });
            }

            if (!aluno.Ativo)
            {
                return BadRequest(new ProblemDetails { Title = "Aluno inativo não pode ser inscrito.", Status = StatusCodes.Status400BadRequest });
            }

            if (!await _context.Cursos.AnyAsync(c => c.Id == cursoId))
            {
                return BadRequest(new ProblemDetails { Title = "Curso não encontrado.", Status = StatusCodes.Status400BadRequest });
            }

            if (await _context.Inscricoes.AnyAsync(i => i.AlunoId == alunoId && i.CursoId == cursoId))
            {
                return Conflict(new ProblemDetails { Title = "O aluno já está inscrito neste curso.", Status = StatusCodes.Status409Conflict });
            }

            var inscricao = new Inscricao { AlunoId = alunoId, CursoId = cursoId };
            _context.Inscricoes.Add(inscricao);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Obter), new { alunoId, cursoId }, inscricao);
        }

        [HttpDelete("{alunoId:int}/{cursoId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Excluir(int alunoId, int cursoId)
        {
            var inscricao = await _context.Inscricoes.FindAsync(alunoId, cursoId);
            if (inscricao is null)
            {
                return NotFound();
            }

            _context.Inscricoes.Remove(inscricao);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}

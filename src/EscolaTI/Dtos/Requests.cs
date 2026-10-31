using System.ComponentModel.DataAnnotations;

namespace EscolaTI.Dtos
{
    public class AlunoRequest
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [MaxLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [MaxLength(254, ErrorMessage = "O e-mail deve ter no máximo 254 caracteres.")]
        [EmailAddress(ErrorMessage = "O e-mail é inválido.")]
        public string Email { get; set; } = string.Empty;

        public bool Ativo { get; set; } = true;
    }

    public class ProfessorRequest
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [MaxLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [MaxLength(254, ErrorMessage = "O e-mail deve ter no máximo 254 caracteres.")]
        [EmailAddress(ErrorMessage = "O e-mail é inválido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A titulação é obrigatória.")]
        [MaxLength(50, ErrorMessage = "A titulação deve ter no máximo 50 caracteres.")]
        public string Titulacao { get; set; } = string.Empty;
    }

    public class CursoRequest
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [MaxLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        // Anulável para que a ausência do campo seja rejeitada em vez de virar 01/01/0001.
        [Required(ErrorMessage = "A data de início é obrigatória.")]
        public DateTime? DataInicio { get; set; }
    }

    public class InscricaoRequest
    {
        [Required(ErrorMessage = "O aluno é obrigatório.")]
        public int? AlunoId { get; set; }

        [Required(ErrorMessage = "O curso é obrigatório.")]
        public int? CursoId { get; set; }
    }
}

using EscolaTI.Models;
using Microsoft.EntityFrameworkCore;

namespace EscolaTI.Data
{
    public class EscolaContext : DbContext
    {
        public EscolaContext(DbContextOptions<EscolaContext> options) : base(options)
        {
        }

        public DbSet<Aluno> Alunos => Set<Aluno>();
        public DbSet<Professor> Professores => Set<Professor>();
        public DbSet<Curso> Cursos => Set<Curso>();
        public DbSet<Inscricao> Inscricoes => Set<Inscricao>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Aluno>(entity =>
            {
                entity.Property(a => a.Nome).HasMaxLength(100);
                entity.Property(a => a.Email).HasMaxLength(254);
                entity.HasIndex(a => a.Email).IsUnique();
            });

            modelBuilder.Entity<Professor>(entity =>
            {
                entity.Property(p => p.Nome).HasMaxLength(100);
                entity.Property(p => p.Email).HasMaxLength(254);
                entity.Property(p => p.Titulacao).HasMaxLength(50);
                entity.HasIndex(p => p.Email).IsUnique();
            });

            modelBuilder.Entity<Curso>(entity =>
            {
                entity.Property(c => c.Nome).HasMaxLength(100);
            });

            // Alunos e cursos com inscrições não podem ser excluídos.
            modelBuilder.Entity<Inscricao>(entity =>
            {
                entity.HasKey(i => new { i.AlunoId, i.CursoId });
                entity.HasOne<Aluno>().WithMany().HasForeignKey(i => i.AlunoId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne<Curso>().WithMany().HasForeignKey(i => i.CursoId).OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}

using System.Net;
using System.Net.Http.Json;
using EscolaTI.Data;
using EscolaTI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

namespace EscolaTI.Tests;

public class ApiTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public ApiTests(ApiFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    private static string NovoEmail() => $"{Guid.NewGuid():N}@escola.com";

    private async Task<Aluno> CriarAluno(bool ativo = true)
    {
        var resposta = await _client.PostAsJsonAsync("/api/alunos", new { nome = "Ana", email = NovoEmail(), ativo });
        resposta.EnsureSuccessStatusCode();
        return (await resposta.Content.ReadFromJsonAsync<Aluno>())!;
    }

    private async Task<Curso> CriarCurso()
    {
        var resposta = await _client.PostAsJsonAsync("/api/cursos", new { nome = "C#", dataInicio = "2027-02-01" });
        resposta.EnsureSuccessStatusCode();
        return (await resposta.Content.ReadFromJsonAsync<Curso>())!;
    }

    [Fact]
    public async Task Aluno_CicloCrudCompleto()
    {
        var email = NovoEmail();
        var criacao = await _client.PostAsJsonAsync("/api/alunos", new { nome = "  Ana  ", email = email.ToUpperInvariant() });
        Assert.Equal(HttpStatusCode.Created, criacao.StatusCode);
        var aluno = (await criacao.Content.ReadFromJsonAsync<Aluno>())!;
        Assert.Equal("Ana", aluno.Nome);
        Assert.Equal(email, aluno.Email);
        Assert.True(aluno.Ativo);
        Assert.Equal($"/api/alunos/{aluno.Id}", criacao.Headers.Location!.AbsolutePath);

        var lista = await _client.GetFromJsonAsync<List<Aluno>>("/api/alunos");
        Assert.Contains(lista!, a => a.Id == aluno.Id);

        var atualizacao = await _client.PutAsJsonAsync($"/api/alunos/{aluno.Id}", new { nome = "Ana Maria", email, ativo = false });
        Assert.Equal(HttpStatusCode.OK, atualizacao.StatusCode);
        var lido = await _client.GetFromJsonAsync<Aluno>($"/api/alunos/{aluno.Id}");
        Assert.Equal("Ana Maria", lido!.Nome);
        Assert.False(lido.Ativo);

        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/alunos/{aluno.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/alunos/{aluno.Id}")).StatusCode);
    }

    [Fact]
    public async Task Professor_CicloCrudCompleto()
    {
        var email = NovoEmail();
        var criacao = await _client.PostAsJsonAsync("/api/professores", new { nome = "Bia", email, titulacao = "Mestre" });
        Assert.Equal(HttpStatusCode.Created, criacao.StatusCode);
        var professor = (await criacao.Content.ReadFromJsonAsync<Professor>())!;
        Assert.Equal("Mestre", professor.Titulacao);

        var atualizacao = await _client.PutAsJsonAsync($"/api/professores/{professor.Id}", new { nome = "Bia", email, titulacao = "Doutora" });
        Assert.Equal(HttpStatusCode.OK, atualizacao.StatusCode);
        var lido = await _client.GetFromJsonAsync<Professor>($"/api/professores/{professor.Id}");
        Assert.Equal("Doutora", lido!.Titulacao);

        var lista = await _client.GetFromJsonAsync<List<Professor>>("/api/professores");
        Assert.Contains(lista!, p => p.Id == professor.Id);

        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/professores/{professor.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/professores/{professor.Id}")).StatusCode);
    }

    [Fact]
    public async Task Curso_CicloCrudCompleto()
    {
        var curso = await CriarCurso();
        Assert.Equal(new DateTime(2027, 2, 1), curso.DataInicio);

        var atualizacao = await _client.PutAsJsonAsync($"/api/cursos/{curso.Id}", new { nome = "C# Avançado", dataInicio = "2027-03-01" });
        Assert.Equal(HttpStatusCode.OK, atualizacao.StatusCode);
        var lido = await _client.GetFromJsonAsync<Curso>($"/api/cursos/{curso.Id}");
        Assert.Equal("C# Avançado", lido!.Nome);

        var lista = await _client.GetFromJsonAsync<List<Curso>>("/api/cursos");
        Assert.Contains(lista!, c => c.Id == curso.Id);

        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/cursos/{curso.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/cursos/{curso.Id}")).StatusCode);
    }

    [Fact]
    public async Task Inscricao_CriaListaObtemEExclui()
    {
        var aluno = await CriarAluno();
        var curso = await CriarCurso();

        var criacao = await _client.PostAsJsonAsync("/api/inscricoes", new { alunoId = aluno.Id, cursoId = curso.Id });
        Assert.Equal(HttpStatusCode.Created, criacao.StatusCode);

        var lista = await _client.GetFromJsonAsync<List<Inscricao>>("/api/inscricoes");
        Assert.Contains(lista!, i => i.AlunoId == aluno.Id && i.CursoId == curso.Id);

        var rota = $"/api/inscricoes/{aluno.Id}/{curso.Id}";
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync(rota)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync(rota)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync(rota)).StatusCode);
    }

    public static TheoryData<string, string, string> PayloadsInvalidos => new()
    {
        { "/api/alunos", """{"email":"a@escola.com"}""", "Nome" },
        { "/api/alunos", """{"nome":"   ","email":"a@escola.com"}""", "Nome" },
        { "/api/alunos", """{"nome":"Ana"}""", "Email" },
        { "/api/alunos", """{"nome":"Ana","email":"invalido"}""", "Email" },
        { "/api/alunos", $$"""{"nome":"{{new string('a', 101)}}","email":"a@escola.com"}""", "Nome" },
        { "/api/professores", """{"nome":"Bia","email":"b@escola.com"}""", "Titulacao" },
        { "/api/professores", """{"nome":"Bia","email":"b@escola.com","titulacao":true}""", "Titulacao" },
        { "/api/cursos", """{"nome":"C#"}""", "DataInicio" },
        { "/api/cursos", """{"nome":"C#","dataInicio":"nao-e-data"}""", "DataInicio" },
        { "/api/inscricoes", """{"cursoId":1}""", "AlunoId" },
        { "/api/inscricoes", """{"alunoId":1}""", "CursoId" },
    };

    [Theory]
    [MemberData(nameof(PayloadsInvalidos))]
    public async Task Post_PayloadInvalido_Retorna400ComOCampo(string rota, string json, string campo)
    {
        var resposta = await _client.PostAsync(rota, new StringContent(json, System.Text.Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains(campo, await resposta.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("/api/alunos")]
    [InlineData("/api/professores")]
    public async Task Post_EmailDuplicadoSemDiferenciarMaiusculas_Retorna409(string rota)
    {
        var email = NovoEmail();
        var corpo = new { nome = "Pessoa", email, titulacao = "Mestre" };
        Assert.Equal(HttpStatusCode.Created, (await _client.PostAsJsonAsync(rota, corpo)).StatusCode);

        var duplicado = await _client.PostAsJsonAsync(rota, corpo with { email = email.ToUpperInvariant() });

        Assert.Equal(HttpStatusCode.Conflict, duplicado.StatusCode);
    }

    [Fact]
    public async Task Put_EmailDeOutroAluno_Retorna409EMesmoEmailEhAceito()
    {
        var primeiro = await CriarAluno();
        var segundo = await CriarAluno();

        var conflito = await _client.PutAsJsonAsync($"/api/alunos/{segundo.Id}", new { nome = "Ana", email = primeiro.Email });
        Assert.Equal(HttpStatusCode.Conflict, conflito.StatusCode);

        var mesmoEmail = await _client.PutAsJsonAsync($"/api/alunos/{segundo.Id}", new { nome = "Outra", email = segundo.Email });
        Assert.Equal(HttpStatusCode.OK, mesmoEmail.StatusCode);
    }

    [Theory]
    [InlineData("/api/alunos/999999")]
    [InlineData("/api/professores/999999")]
    [InlineData("/api/cursos/999999")]
    [InlineData("/api/inscricoes/999999/999999")]
    public async Task RecursoInexistente_Retorna404(string rota)
    {
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync(rota)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.DeleteAsync(rota)).StatusCode);
    }

    [Theory]
    [InlineData("/api/alunos/999999", """{"nome":"X","email":"x@escola.com"}""")]
    [InlineData("/api/professores/999999", """{"nome":"X","email":"x@escola.com","titulacao":"Mestre"}""")]
    [InlineData("/api/cursos/999999", """{"nome":"X","dataInicio":"2027-01-01"}""")]
    public async Task Put_RecursoInexistente_Retorna404(string rota, string json)
    {
        var resposta = await _client.PutAsync(rota, new StringContent(json, System.Text.Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task Inscricao_RegrasDeNegocio()
    {
        var aluno = await CriarAluno();
        var inativo = await CriarAluno(ativo: false);
        var curso = await CriarCurso();

        Assert.Equal(HttpStatusCode.BadRequest, (await _client.PostAsJsonAsync("/api/inscricoes", new { alunoId = 999999, cursoId = curso.Id })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await _client.PostAsJsonAsync("/api/inscricoes", new { alunoId = aluno.Id, cursoId = 999999 })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await _client.PostAsJsonAsync("/api/inscricoes", new { alunoId = inativo.Id, cursoId = curso.Id })).StatusCode);

        var inscricao = new { alunoId = aluno.Id, cursoId = curso.Id };
        Assert.Equal(HttpStatusCode.Created, (await _client.PostAsJsonAsync("/api/inscricoes", inscricao)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await _client.PostAsJsonAsync("/api/inscricoes", inscricao)).StatusCode);
    }

    [Fact]
    public async Task Excluir_AlunoOuCursoComInscricao_Retorna409()
    {
        var aluno = await CriarAluno();
        var curso = await CriarCurso();
        await _client.PostAsJsonAsync("/api/inscricoes", new { alunoId = aluno.Id, cursoId = curso.Id });

        Assert.Equal(HttpStatusCode.Conflict, (await _client.DeleteAsync($"/api/alunos/{aluno.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await _client.DeleteAsync($"/api/cursos/{curso.Id}")).StatusCode);
    }

    [Fact]
    public async Task Swagger_DocumentaOsEndpoints()
    {
        var documento = await _client.GetStringAsync("/swagger/v1/swagger.json");

        foreach (var rota in new[] { "/api/alunos", "/api/professores", "/api/cursos", "/api/inscricoes" })
        {
            Assert.Contains(rota, documento);
        }
    }

    [Fact]
    public async Task ExceptionHandler_ConverteFalhaDoBancoEm409()
    {
        var contexto = new DefaultHttpContext();
        contexto.Response.Body = new MemoryStream();

        var tratada = await new ConflitoBancoExceptionHandler().TryHandleAsync(contexto, new DbUpdateException("falha"), CancellationToken.None);
        var ignorada = await new ConflitoBancoExceptionHandler().TryHandleAsync(new DefaultHttpContext(), new InvalidOperationException(), CancellationToken.None);

        Assert.True(tratada);
        Assert.Equal(StatusCodes.Status409Conflict, contexto.Response.StatusCode);
        Assert.False(ignorada);
    }
}

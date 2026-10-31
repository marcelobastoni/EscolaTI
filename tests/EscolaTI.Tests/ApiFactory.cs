using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;

namespace EscolaTI.Tests;

// Sobe a API real com as migrações aplicadas em um arquivo SQLite temporário.
public class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _banco = Path.Combine(Path.GetTempPath(), $"escolati-testes-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:EscolaTI", $"Data Source={_banco}");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        SqliteConnection.ClearAllPools();
        File.Delete(_banco);
    }
}

using EscolaTI.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<EscolaContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("EscolaTI")));
builder.Services.AddExceptionHandler<ConflitoBancoExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Cria ou atualiza o banco na inicialização.
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<EscolaContext>().Database.Migrate();
}

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

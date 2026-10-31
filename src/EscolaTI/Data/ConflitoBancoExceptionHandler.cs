using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EscolaTI.Data
{
    // Requisições simultâneas podem passar pelas verificações dos controllers e só falhar
    // nas restrições do banco (e-mail único, chaves); isso é um conflito, não um erro 500.
    public class ConflitoBancoExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is not DbUpdateException)
            {
                return false;
            }

            httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
            await httpContext.Response.WriteAsJsonAsync(
                new ProblemDetails { Title = "A operação conflita com os dados existentes.", Status = StatusCodes.Status409Conflict },
                options: null,
                contentType: "application/problem+json",
                cancellationToken);
            return true;
        }
    }
}

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace DominoPontaDeQuina.WebApi.Infrastructure;

/// <summary>Traduz as exceções da camada de aplicação em respostas ProblemDetails.</summary>
public sealed class ApiExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    /// <summary>Tenta converter a exceção em uma resposta HTTP conhecida.</summary>
    /// <param name="httpContext">Contexto da requisição.</param>
    /// <param name="exception">Exceção capturada no pipeline.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns><c>true</c> quando a exceção foi tratada.</returns>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, titulo) = Mapear(exception);
        if (status is null)
            return false;

        logger.LogWarning(exception, "Requisição rejeitada com status {Status}.", status);
        httpContext.Response.StatusCode = status.Value;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = titulo,
                Detail = exception.Message
            }
        });
    }

    static (int? Status, string? Titulo) Mapear(Exception exception) => exception switch
    {
        KeyNotFoundException => (StatusCodes.Status404NotFound, "Recurso não encontrado."),
        ArgumentException => (StatusCodes.Status400BadRequest, "Requisição inválida."),
        InvalidOperationException => (StatusCodes.Status409Conflict, "Operação não permitida no estado atual."),
        _ => (null, null)
    };
}

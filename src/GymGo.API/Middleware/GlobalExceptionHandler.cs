using FluentValidation;
using GymGo.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace GymGo.API.Middleware;

/// <summary>
/// Captura excepciones no manejadas y las traduce a ProblemDetails.
/// Registrado vía AddExceptionHandler + UseExceptionHandler.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title, detail, extensions) = exception switch
        {
            ValidationException ve => (
                StatusCodes.Status400BadRequest,
                "Validación fallida",
                "Una o más reglas de validación fallaron.",
                (object?)ve.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
            ),
            NotFoundException nf => (
                StatusCodes.Status404NotFound,
                "Recurso no encontrado",
                nf.Message,
                (object?)null
            ),
            BusinessRuleViolationException br => (
                StatusCodes.Status422UnprocessableEntity,
                "Regla de negocio violada",
                br.Message,
                (object?)new { code = br.Code }
            ),
            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                "No autorizado",
                "Se requieren credenciales válidas.",
                (object?)null
            ),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Error interno",
                "Ocurrió un error inesperado.",
                (object?)null
            )
        };

        if (status >= 500)
            _logger.LogError(exception, "Excepción no manejada: {Message}", exception.Message);
        else
            _logger.LogWarning(exception, "Excepción manejada ({Status}): {Message}", status, exception.Message);

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        if (extensions is not null)
            problem.Extensions["errors"] = extensions;

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }
}

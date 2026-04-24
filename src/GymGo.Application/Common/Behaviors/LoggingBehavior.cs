using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GymGo.Application.Common.Behaviors;

/// <summary>
/// Loggea inicio, fin, duración y request name de cada Command/Query.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation("[BEGIN] {RequestName}", requestName);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await next();
            stopwatch.Stop();
            _logger.LogInformation("[END] {RequestName} ({ElapsedMs} ms)", requestName, stopwatch.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "[ERROR] {RequestName} ({ElapsedMs} ms): {Message}",
                requestName, stopwatch.ElapsedMilliseconds, ex.Message);
            throw;
        }
    }
}

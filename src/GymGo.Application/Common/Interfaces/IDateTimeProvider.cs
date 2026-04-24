namespace GymGo.Application.Common.Interfaces;

/// <summary>
/// Proveedor de fecha/hora. Permite mockear "ahora" en tests.
/// </summary>
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
    DateOnly TodayUtc { get; }
}

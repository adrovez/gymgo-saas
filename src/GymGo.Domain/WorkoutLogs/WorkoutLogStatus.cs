namespace GymGo.Domain.WorkoutLogs;

/// <summary>
/// Estado de un registro de rutina diaria.
/// </summary>
public enum WorkoutLogStatus
{
    /// <summary>Rutina en curso o registrada parcialmente.</summary>
    Draft = 0,

    /// <summary>Rutina finalizada y confirmada por el socio o el staff.</summary>
    Completed = 1
}

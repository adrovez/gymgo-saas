namespace GymGo.Domain.WorkoutLogs;

/// <summary>
/// Grupo muscular principal trabajado en un ejercicio.
/// </summary>
public enum MuscleGroup
{
    /// <summary>No especificado o ejercicio multi-articular sin categoría clara.</summary>
    NotSpecified = 0,

    /// <summary>Pecho (pectorales mayor y menor).</summary>
    Chest = 1,

    /// <summary>Espalda (dorsales, trapecios, romboides).</summary>
    Back = 2,

    /// <summary>Hombros (deltoides).</summary>
    Shoulders = 3,

    /// <summary>Bíceps.</summary>
    Biceps = 4,

    /// <summary>Tríceps.</summary>
    Triceps = 5,

    /// <summary>Piernas (cuádriceps, isquiotibiales, pantorrillas).</summary>
    Legs = 6,

    /// <summary>Core / abdomen (recto abdominal, oblicuos, transverso).</summary>
    Core = 7,

    /// <summary>Glúteos.</summary>
    Glutes = 8,

    /// <summary>Cardio (ejercicios aeróbicos de resistencia).</summary>
    Cardio = 9,

    /// <summary>Cuerpo completo / funcional.</summary>
    FullBody = 10
}

-- =============================================================================
--  GymGo  ·  01_schema / 13_WorkoutRoutines.sql
--  Rutinas de entrenamiento por socio y registro de avances.
--
--  MIGRACIÓN: Elimina las tablas anteriores (WorkoutLogExercises, WorkoutLogs)
--  y recrea el módulo con el modelo correcto de Rutinas:
--
--    WorkoutPlans          → Rutina asignada al socio con período y medidas
--    WorkoutPlanDays       → Días de la semana configurados en la rutina
--    WorkoutPlanExercises  → Ejercicios planificados por día
--    WorkoutLogs           → Sesión de entrenamiento (registro de avances)
--    WorkoutLogExercises   → Ejercicios realizados vs planificados
--
--  WorkoutPlanStatus (enum, int):
--      0 = Active      → Rutina vigente
--      1 = Completed   → Rutina completada al vencer el período
--      2 = Cancelled   → Rutina cancelada
--
--  DayOfWeek (enum, int):
--      1 = Monday      → Lunes
--      2 = Tuesday     → Martes
--      3 = Wednesday   → Miércoles
--      4 = Thursday    → Jueves
--      5 = Friday      → Viernes
--      6 = Saturday    → Sábado
--      7 = Sunday      → Domingo
--
--  WorkoutLogStatus (enum, int):
--      0 = Draft       → Sesión en curso / incompleta
--      1 = Completed   → Sesión finalizada y registrada
--
--  MuscleGroup (enum, int):
--      0  = NotSpecified
--      1  = Chest          → Pecho
--      2  = Back           → Espalda
--      3  = Shoulders      → Hombros
--      4  = Biceps
--      5  = Triceps
--      6  = Legs           → Piernas
--      7  = Core           → Core / abdomen
--      8  = Glutes         → Glúteos
--      9  = Cardio         → Ejercicios cardiovasculares
--      10 = FullBody       → Cuerpo completo / funcional
-- =============================================================================

USE [GymGoDb_Dev];
GO

-- =============================================================================
--  0.  MIGRACIÓN — Eliminar tablas anteriores
-- =============================================================================

-- Eliminar en orden inverso de dependencia FK
IF OBJECT_ID(N'[dbo].[WorkoutLogExercises]', N'U') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[WorkoutLogExercises];
    PRINT 'Tabla [dbo].[WorkoutLogExercises] eliminada.';
END
GO

IF OBJECT_ID(N'[dbo].[WorkoutLogs]', N'U') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[WorkoutLogs];
    PRINT 'Tabla [dbo].[WorkoutLogs] eliminada.';
END
GO

-- =============================================================================
--  1.  WorkoutPlans  (cabecera de la rutina asignada al socio)
-- =============================================================================

IF OBJECT_ID(N'[dbo].[WorkoutPlans]', N'U') IS NOT NULL
BEGIN
    PRINT 'La tabla [dbo].[WorkoutPlans] ya existe. Sin cambios.';
END
ELSE
BEGIN
    CREATE TABLE [dbo].[WorkoutPlans]
    (
        -- ── Identidad ─────────────────────────────────────────────────────────
        [Id]                        UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_WorkoutPlans] PRIMARY KEY,
        [TenantId]                  UNIQUEIDENTIFIER NOT NULL,
        [MemberId]                  UNIQUEIDENTIFIER NOT NULL,

        -- ── Datos de la rutina ────────────────────────────────────────────────
        [Objective]                 NVARCHAR(500)    NOT NULL,   -- Objetivo de la rutina
        [StartDate]                 DATE             NOT NULL,   -- Inicio del período
        [EndDate]                   DATE             NOT NULL,   -- Fin del período
        [Notes]                     NVARCHAR(1000)   NULL,       -- Observaciones del instructor

        -- ── Medidas físicas iniciales ─────────────────────────────────────────
        [InitialWeightKg]           DECIMAL(5, 2)    NULL,       -- Peso inicial (kg)
        [InitialHeightCm]           DECIMAL(5, 2)    NULL,       -- Estatura (cm)
        [InitialBodyFatPercentage]  DECIMAL(5, 2)    NULL,       -- % grasa corporal inicial

        -- ── Estado ────────────────────────────────────────────────────────────
        [Status]                    INT              NOT NULL CONSTRAINT [DF_WorkoutPlans_Status] DEFAULT (0),
                                                                 -- WorkoutPlanStatus enum

        -- ── IAuditable ────────────────────────────────────────────────────────
        [CreatedAtUtc]              DATETIME2(3)     NOT NULL,
        [CreatedBy]                 NVARCHAR(200)    NULL,
        [ModifiedAtUtc]             DATETIME2(3)     NULL,
        [ModifiedBy]                NVARCHAR(200)    NULL,

        -- ── ISoftDeletable ────────────────────────────────────────────────────
        [IsDeleted]                 BIT              NOT NULL CONSTRAINT [DF_WorkoutPlans_IsDeleted] DEFAULT (0),
        [DeletedAtUtc]              DATETIME2(3)     NULL,
        [DeletedBy]                 NVARCHAR(200)    NULL,

        -- ── FK físicas ────────────────────────────────────────────────────────
        CONSTRAINT [FK_WorkoutPlans_Tenants]
            FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants] ([Id]),

        CONSTRAINT [FK_WorkoutPlans_Members]
            FOREIGN KEY ([MemberId]) REFERENCES [dbo].[Members] ([Id]),

        -- ── CHECK constraints ─────────────────────────────────────────────────
        CONSTRAINT [CK_WorkoutPlans_Status]
            CHECK ([Status] IN (0, 1, 2)),

        CONSTRAINT [CK_WorkoutPlans_Dates]
            CHECK ([EndDate] >= [StartDate]),

        CONSTRAINT [CK_WorkoutPlans_Weight]
            CHECK ([InitialWeightKg] IS NULL OR [InitialWeightKg] > 0),

        CONSTRAINT [CK_WorkoutPlans_Height]
            CHECK ([InitialHeightCm] IS NULL OR [InitialHeightCm] > 0),

        CONSTRAINT [CK_WorkoutPlans_BodyFat]
            CHECK ([InitialBodyFatPercentage] IS NULL OR
                   ([InitialBodyFatPercentage] >= 0 AND [InitialBodyFatPercentage] <= 100))
    );

    -- Historial de rutinas por socio (filtro más frecuente)
    CREATE NONCLUSTERED INDEX [IX_WorkoutPlans_TenantId]
        ON [dbo].[WorkoutPlans] ([TenantId] ASC);

    CREATE NONCLUSTERED INDEX [IX_WorkoutPlans_Member_Status]
        ON [dbo].[WorkoutPlans] ([TenantId] ASC, [MemberId] ASC, [Status] ASC)
        WHERE [IsDeleted] = 0;

    PRINT 'Tabla [dbo].[WorkoutPlans] creada.';
END
GO

-- =============================================================================
--  2.  WorkoutPlanDays  (días de la semana configurados en la rutina)
-- =============================================================================

IF OBJECT_ID(N'[dbo].[WorkoutPlanDays]', N'U') IS NOT NULL
BEGIN
    PRINT 'La tabla [dbo].[WorkoutPlanDays] ya existe. Sin cambios.';
END
ELSE
BEGIN
    CREATE TABLE [dbo].[WorkoutPlanDays]
    (
        -- ── Identidad ─────────────────────────────────────────────────────────
        [Id]            UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_WorkoutPlanDays] PRIMARY KEY,
        [WorkoutPlanId] UNIQUEIDENTIFIER NOT NULL,

        -- ── Día de la semana ──────────────────────────────────────────────────
        [DayOfWeek]     INT              NOT NULL,               -- DayOfWeek enum: 1=Lunes … 7=Domingo
        [Notes]         NVARCHAR(500)    NULL,                   -- Descripción opcional (ej: "Pecho/Tríceps")

        -- ── FK físicas ────────────────────────────────────────────────────────
        CONSTRAINT [FK_WorkoutPlanDays_WorkoutPlans]
            FOREIGN KEY ([WorkoutPlanId]) REFERENCES [dbo].[WorkoutPlans] ([Id])
            ON DELETE CASCADE,

        -- ── CHECK constraints ─────────────────────────────────────────────────
        CONSTRAINT [CK_WorkoutPlanDays_DayOfWeek]
            CHECK ([DayOfWeek] BETWEEN 1 AND 7),

        -- Cada día de la semana aparece una sola vez por rutina
        CONSTRAINT [UQ_WorkoutPlanDays_PlanDay]
            UNIQUE ([WorkoutPlanId], [DayOfWeek])
    );

    CREATE NONCLUSTERED INDEX [IX_WorkoutPlanDays_WorkoutPlanId]
        ON [dbo].[WorkoutPlanDays] ([WorkoutPlanId] ASC, [DayOfWeek] ASC);

    PRINT 'Tabla [dbo].[WorkoutPlanDays] creada.';
END
GO

-- =============================================================================
--  3.  WorkoutPlanExercises  (ejercicios planificados por día)
-- =============================================================================

IF OBJECT_ID(N'[dbo].[WorkoutPlanExercises]', N'U') IS NOT NULL
BEGIN
    PRINT 'La tabla [dbo].[WorkoutPlanExercises] ya existe. Sin cambios.';
END
ELSE
BEGIN
    CREATE TABLE [dbo].[WorkoutPlanExercises]
    (
        -- ── Identidad ─────────────────────────────────────────────────────────
        [Id]                    UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_WorkoutPlanExercises] PRIMARY KEY,
        [WorkoutPlanDayId]      UNIQUEIDENTIFIER NOT NULL,

        -- ── Ejercicio ─────────────────────────────────────────────────────────
        [ExerciseName]          NVARCHAR(200)    NOT NULL,       -- Nombre del ejercicio (texto libre)
        [MuscleGroup]           INT              NOT NULL CONSTRAINT [DF_WPE_MuscleGroup] DEFAULT (0),
                                                                 -- MuscleGroup enum
        [SortOrder]             INT              NOT NULL CONSTRAINT [DF_WPE_SortOrder] DEFAULT (0),

        -- ── Métricas planificadas ─────────────────────────────────────────────
        [PlannedSets]           INT              NULL,           -- Series planificadas
        [PlannedReps]           INT              NULL,           -- Repeticiones por serie
        [PlannedWeightKg]       DECIMAL(6, 2)    NULL,           -- Peso planificado (kg)
        [PlannedDurationMinutes] INT             NULL,           -- Duración planificada (minutos)
        [PlannedDistanceMeters] INT              NULL,           -- Distancia planificada (metros)

        [Notes]                 NVARCHAR(500)    NULL,

        -- ── FK físicas ────────────────────────────────────────────────────────
        CONSTRAINT [FK_WorkoutPlanExercises_WorkoutPlanDays]
            FOREIGN KEY ([WorkoutPlanDayId]) REFERENCES [dbo].[WorkoutPlanDays] ([Id])
            ON DELETE CASCADE,

        -- ── CHECK constraints ─────────────────────────────────────────────────
        CONSTRAINT [CK_WPE_MuscleGroup]
            CHECK ([MuscleGroup] IN (0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10)),

        CONSTRAINT [CK_WPE_Sets]
            CHECK ([PlannedSets] IS NULL OR [PlannedSets] > 0),

        CONSTRAINT [CK_WPE_Reps]
            CHECK ([PlannedReps] IS NULL OR [PlannedReps] > 0),

        CONSTRAINT [CK_WPE_WeightKg]
            CHECK ([PlannedWeightKg] IS NULL OR [PlannedWeightKg] >= 0),

        CONSTRAINT [CK_WPE_Duration]
            CHECK ([PlannedDurationMinutes] IS NULL OR [PlannedDurationMinutes] > 0),

        CONSTRAINT [CK_WPE_Distance]
            CHECK ([PlannedDistanceMeters] IS NULL OR [PlannedDistanceMeters] > 0)
    );

    CREATE NONCLUSTERED INDEX [IX_WorkoutPlanExercises_DayId]
        ON [dbo].[WorkoutPlanExercises] ([WorkoutPlanDayId] ASC, [SortOrder] ASC);

    PRINT 'Tabla [dbo].[WorkoutPlanExercises] creada.';
END
GO

-- =============================================================================
--  4.  WorkoutLogs  (sesión de entrenamiento — registro de avances del socio)
-- =============================================================================

IF OBJECT_ID(N'[dbo].[WorkoutLogs]', N'U') IS NOT NULL
BEGIN
    PRINT 'La tabla [dbo].[WorkoutLogs] ya existe. Sin cambios.';
END
ELSE
BEGIN
    CREATE TABLE [dbo].[WorkoutLogs]
    (
        -- ── Identidad ─────────────────────────────────────────────────────────
        [Id]                UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_WorkoutLogs] PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER NOT NULL,
        [MemberId]          UNIQUEIDENTIFIER NOT NULL,

        -- ── Referencia a la rutina ────────────────────────────────────────────
        [WorkoutPlanId]     UNIQUEIDENTIFIER NOT NULL,           -- Rutina que se está ejecutando
        [WorkoutPlanDayId]  UNIQUEIDENTIFIER NOT NULL,           -- Día de la rutina realizado

        -- ── Datos de la sesión ────────────────────────────────────────────────
        [Date]              DATE             NOT NULL,           -- Fecha en que se realizó
        [Notes]             NVARCHAR(1000)   NULL,               -- Observaciones del socio
        [Status]            INT              NOT NULL CONSTRAINT [DF_WorkoutLogs_Status] DEFAULT (0),
                                                                 -- WorkoutLogStatus enum

        -- ── IAuditable ────────────────────────────────────────────────────────
        [CreatedAtUtc]      DATETIME2(3)     NOT NULL,
        [CreatedBy]         NVARCHAR(200)    NULL,
        [ModifiedAtUtc]     DATETIME2(3)     NULL,
        [ModifiedBy]        NVARCHAR(200)    NULL,

        -- ── ISoftDeletable ────────────────────────────────────────────────────
        [IsDeleted]         BIT              NOT NULL CONSTRAINT [DF_WorkoutLogs_IsDeleted] DEFAULT (0),
        [DeletedAtUtc]      DATETIME2(3)     NULL,
        [DeletedBy]         NVARCHAR(200)    NULL,

        -- ── FK físicas ────────────────────────────────────────────────────────
        CONSTRAINT [FK_WorkoutLogs_Tenants]
            FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants] ([Id]),

        CONSTRAINT [FK_WorkoutLogs_Members]
            FOREIGN KEY ([MemberId]) REFERENCES [dbo].[Members] ([Id]),

        CONSTRAINT [FK_WorkoutLogs_WorkoutPlans]
            FOREIGN KEY ([WorkoutPlanId]) REFERENCES [dbo].[WorkoutPlans] ([Id]),

        CONSTRAINT [FK_WorkoutLogs_WorkoutPlanDays]
            FOREIGN KEY ([WorkoutPlanDayId]) REFERENCES [dbo].[WorkoutPlanDays] ([Id]),

        -- ── CHECK constraints ─────────────────────────────────────────────────
        CONSTRAINT [CK_WorkoutLogs_Status]
            CHECK ([Status] IN (0, 1))
    );

    -- Historial del socio ordenado por fecha
    CREATE NONCLUSTERED INDEX [IX_WorkoutLogs_Member_Date]
        ON [dbo].[WorkoutLogs] ([TenantId] ASC, [MemberId] ASC, [Date] DESC)
        WHERE [IsDeleted] = 0;

    -- Sesiones por rutina (para ver progreso de un plan)
    CREATE NONCLUSTERED INDEX [IX_WorkoutLogs_WorkoutPlanId]
        ON [dbo].[WorkoutLogs] ([WorkoutPlanId] ASC, [Date] ASC)
        WHERE [IsDeleted] = 0;

    -- Sesiones por día de rutina (para comparar avances del mismo día)
    CREATE NONCLUSTERED INDEX [IX_WorkoutLogs_WorkoutPlanDayId]
        ON [dbo].[WorkoutLogs] ([WorkoutPlanDayId] ASC, [Date] ASC)
        WHERE [IsDeleted] = 0;

    PRINT 'Tabla [dbo].[WorkoutLogs] creada.';
END
GO

-- =============================================================================
--  5.  WorkoutLogExercises  (ejercicios realizados en la sesión)
-- =============================================================================

IF OBJECT_ID(N'[dbo].[WorkoutLogExercises]', N'U') IS NOT NULL
BEGIN
    PRINT 'La tabla [dbo].[WorkoutLogExercises] ya existe. Sin cambios.';
END
ELSE
BEGIN
    CREATE TABLE [dbo].[WorkoutLogExercises]
    (
        -- ── Identidad ─────────────────────────────────────────────────────────
        [Id]                    UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_WorkoutLogExercises] PRIMARY KEY,
        [WorkoutLogId]          UNIQUEIDENTIFIER NOT NULL,

        -- ── Referencia al ejercicio planificado (NULL si es ejercicio extra) ──
        [WorkoutPlanExerciseId] UNIQUEIDENTIFIER NULL,

        -- ── Ejercicio ─────────────────────────────────────────────────────────
        [ExerciseName]          NVARCHAR(200)    NOT NULL,       -- Copia del nombre (preserva el registro)
        [MuscleGroup]           INT              NOT NULL CONSTRAINT [DF_WLE_MuscleGroup] DEFAULT (0),
        [SortOrder]             INT              NOT NULL CONSTRAINT [DF_WLE_SortOrder] DEFAULT (0),
        [IsExtra]               BIT              NOT NULL CONSTRAINT [DF_WLE_IsExtra] DEFAULT (0),
                                                                 -- 1 = ejercicio no planificado

        -- ── Métricas realizadas ───────────────────────────────────────────────
        [ActualSets]            INT              NULL,           -- Series realizadas
        [ActualReps]            INT              NULL,           -- Repeticiones realizadas
        [ActualWeightKg]        DECIMAL(6, 2)    NULL,           -- Peso utilizado (kg)
        [ActualDurationMinutes] INT              NULL,           -- Duración realizada (minutos)
        [ActualDistanceMeters]  INT              NULL,           -- Distancia realizada (metros)

        [Notes]                 NVARCHAR(500)    NULL,

        -- ── FK físicas ────────────────────────────────────────────────────────
        CONSTRAINT [FK_WorkoutLogExercises_WorkoutLogs]
            FOREIGN KEY ([WorkoutLogId]) REFERENCES [dbo].[WorkoutLogs] ([Id])
            ON DELETE CASCADE,

        CONSTRAINT [FK_WorkoutLogExercises_WorkoutPlanExercises]
            FOREIGN KEY ([WorkoutPlanExerciseId]) REFERENCES [dbo].[WorkoutPlanExercises] ([Id]),

        -- ── CHECK constraints ─────────────────────────────────────────────────
        CONSTRAINT [CK_WLE_MuscleGroup]
            CHECK ([MuscleGroup] IN (0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10)),

        CONSTRAINT [CK_WLE_Sets]
            CHECK ([ActualSets] IS NULL OR [ActualSets] > 0),

        CONSTRAINT [CK_WLE_Reps]
            CHECK ([ActualReps] IS NULL OR [ActualReps] > 0),

        CONSTRAINT [CK_WLE_WeightKg]
            CHECK ([ActualWeightKg] IS NULL OR [ActualWeightKg] >= 0),

        CONSTRAINT [CK_WLE_Duration]
            CHECK ([ActualDurationMinutes] IS NULL OR [ActualDurationMinutes] > 0),

        CONSTRAINT [CK_WLE_Distance]
            CHECK ([ActualDistanceMeters] IS NULL OR [ActualDistanceMeters] > 0)
    );

    CREATE NONCLUSTERED INDEX [IX_WorkoutLogExercises_WorkoutLogId]
        ON [dbo].[WorkoutLogExercises] ([WorkoutLogId] ASC, [SortOrder] ASC);

    PRINT 'Tabla [dbo].[WorkoutLogExercises] creada.';
END
GO

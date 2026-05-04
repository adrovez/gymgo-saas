-- =============================================================================
--  GymGo  ·  01_schema / 12_WorkoutLogs.sql
--  Registro de rutinas diarias y avances de ejercicios por socio.
--
--  WorkoutLogStatus (enum, int):
--      0 = Draft       → Rutina en curso / incompleta
--      1 = Completed   → Rutina finalizada y registrada
--
--  MuscleGroup (enum, int):
--      0  = NotSpecified
--      1  = Chest          → Pecho
--      2  = Back           → Espalda
--      3  = Shoulders      → Hombros
--      4  = Biceps
--      5  = Triceps
--      6  = Legs           → Piernas (cuádriceps, isquiotibiales, pantorrillas)
--      7  = Core           → Core / abdomen
--      8  = Glutes         → Glúteos
--      9  = Cardio         → Ejercicios cardiovasculares
--      10 = FullBody       → Cuerpo completo / funcional
-- =============================================================================

USE [GymGoDb_Dev];
GO

-- ─────────────────────────────────────────────────────────────────────────────
--  1.  WorkoutLogs  (cabecera de la sesión de entrenamiento)
-- ─────────────────────────────────────────────────────────────────────────────

IF OBJECT_ID(N'[dbo].[WorkoutLogs]', N'U') IS NOT NULL
BEGIN
    PRINT 'La tabla [dbo].[WorkoutLogs] ya existe. Sin cambios.';
END
ELSE
BEGIN
    CREATE TABLE [dbo].[WorkoutLogs]
    (
        -- ── Identidad ─────────────────────────────────────────────────────────
        [Id]            UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_WorkoutLogs] PRIMARY KEY,
        [TenantId]      UNIQUEIDENTIFIER NOT NULL,
        [MemberId]      UNIQUEIDENTIFIER NOT NULL,

        -- ── Datos de la sesión ────────────────────────────────────────────────
        [Date]          DATE             NOT NULL,            -- Fecha de entrenamiento
        [Title]         NVARCHAR(200)    NULL,                -- Título opcional (ej: "Día A - Push")
        [Notes]         NVARCHAR(1000)   NULL,                -- Observaciones generales de la sesión
        [Status]        INT              NOT NULL CONSTRAINT [DF_WorkoutLogs_Status] DEFAULT (0),
                                                              -- WorkoutLogStatus enum

        -- ── IAuditable ────────────────────────────────────────────────────────
        [CreatedAtUtc]  DATETIME2(3)     NOT NULL,
        [CreatedBy]     NVARCHAR(200)    NULL,
        [ModifiedAtUtc] DATETIME2(3)     NULL,
        [ModifiedBy]    NVARCHAR(200)    NULL,

        -- ── ISoftDeletable ────────────────────────────────────────────────────
        [IsDeleted]     BIT              NOT NULL CONSTRAINT [DF_WorkoutLogs_IsDeleted] DEFAULT (0),
        [DeletedAtUtc]  DATETIME2(3)     NULL,
        [DeletedBy]     NVARCHAR(200)    NULL,

        -- ── FK físicas ────────────────────────────────────────────────────────
        CONSTRAINT [FK_WorkoutLogs_Tenants]
            FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants] ([Id]),

        CONSTRAINT [FK_WorkoutLogs_Members]
            FOREIGN KEY ([MemberId]) REFERENCES [dbo].[Members] ([Id]),

        -- ── CHECK constraints ─────────────────────────────────────────────────
        CONSTRAINT [CK_WorkoutLogs_Status]
            CHECK ([Status] IN (0, 1))
    );

    -- Índice por tenant (listados generales del gym)
    CREATE NONCLUSTERED INDEX [IX_WorkoutLogs_TenantId]
        ON [dbo].[WorkoutLogs] ([TenantId] ASC);

    -- Índice compuesto para historial de un socio ordenado por fecha
    CREATE NONCLUSTERED INDEX [IX_WorkoutLogs_Member_Date]
        ON [dbo].[WorkoutLogs] ([TenantId] ASC, [MemberId] ASC, [Date] DESC)
        WHERE [IsDeleted] = 0;

    -- Índice para filtrar por tenant + fecha (reporte del día)
    CREATE NONCLUSTERED INDEX [IX_WorkoutLogs_Tenant_Date]
        ON [dbo].[WorkoutLogs] ([TenantId] ASC, [Date] ASC)
        WHERE [IsDeleted] = 0;

    PRINT 'Tabla [dbo].[WorkoutLogs] creada.';
END
GO

-- ─────────────────────────────────────────────────────────────────────────────
--  2.  WorkoutLogExercises  (ejercicios de cada sesión)
-- ─────────────────────────────────────────────────────────────────────────────

IF OBJECT_ID(N'[dbo].[WorkoutLogExercises]', N'U') IS NOT NULL
BEGIN
    PRINT 'La tabla [dbo].[WorkoutLogExercises] ya existe. Sin cambios.';
END
ELSE
BEGIN
    CREATE TABLE [dbo].[WorkoutLogExercises]
    (
        -- ── Identidad ─────────────────────────────────────────────────────────
        [Id]              UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_WorkoutLogExercises] PRIMARY KEY,
        [WorkoutLogId]    UNIQUEIDENTIFIER NOT NULL,

        -- ── Ejercicio ─────────────────────────────────────────────────────────
        [ExerciseName]    NVARCHAR(200)    NOT NULL,          -- Nombre del ejercicio
        [MuscleGroup]     INT              NOT NULL CONSTRAINT [DF_WLE_MuscleGroup] DEFAULT (0),
                                                              -- MuscleGroup enum
        [SortOrder]       INT              NOT NULL CONSTRAINT [DF_WLE_SortOrder] DEFAULT (0),

        -- ── Métricas de la serie ──────────────────────────────────────────────
        [Sets]            INT              NULL,              -- Número de series realizadas
        [Reps]            INT              NULL,              -- Repeticiones por serie
        [WeightKg]        DECIMAL(6, 2)    NULL,              -- Peso utilizado (kg)
        [DurationSeconds] INT              NULL,              -- Duración (ejercicios de tiempo)
        [DistanceMeters]  DECIMAL(8, 2)    NULL,              -- Distancia (cardio, en metros)

        -- ── Observaciones ─────────────────────────────────────────────────────
        [Notes]           NVARCHAR(500)    NULL,

        -- ── FK físicas ────────────────────────────────────────────────────────
        CONSTRAINT [FK_WorkoutLogExercises_WorkoutLogs]
            FOREIGN KEY ([WorkoutLogId]) REFERENCES [dbo].[WorkoutLogs] ([Id])
            ON DELETE CASCADE,

        -- ── CHECK constraints ─────────────────────────────────────────────────
        CONSTRAINT [CK_WLE_MuscleGroup]
            CHECK ([MuscleGroup] IN (0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10)),

        CONSTRAINT [CK_WLE_Sets]
            CHECK ([Sets] IS NULL OR [Sets] > 0),

        CONSTRAINT [CK_WLE_Reps]
            CHECK ([Reps] IS NULL OR [Reps] > 0),

        CONSTRAINT [CK_WLE_WeightKg]
            CHECK ([WeightKg] IS NULL OR [WeightKg] >= 0),

        CONSTRAINT [CK_WLE_DurationSeconds]
            CHECK ([DurationSeconds] IS NULL OR [DurationSeconds] > 0),

        CONSTRAINT [CK_WLE_DistanceMeters]
            CHECK ([DistanceMeters] IS NULL OR [DistanceMeters] > 0)
    );

    -- Índice para cargar todos los ejercicios de una sesión ordenados
    CREATE NONCLUSTERED INDEX [IX_WorkoutLogExercises_WorkoutLogId]
        ON [dbo].[WorkoutLogExercises] ([WorkoutLogId] ASC, [SortOrder] ASC);

    PRINT 'Tabla [dbo].[WorkoutLogExercises] creada.';
END
GO

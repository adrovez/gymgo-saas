-- =============================================================================
--  GymGo  ·  01_schema / 06_GymClasses.sql
--  Clases del gimnasio y sus horarios semanales recurrentes.
--
--  GymClasses   → catálogo de tipos de clase (Yoga, Spinning, Box Funcional…)
--  ClassSchedules → slots semanales recurrentes (Lunes 07:00, Miércoles 09:00…)
--
--  ClassCategory (enum, int):
--      0 = Other        → Otro / sin categoría
--      1 = Cardio       → Cardio / aeróbico
--      2 = Strength     → Fuerza / musculación
--      3 = Flexibility  → Flexibilidad / movilidad
--      4 = Martial      → Artes marciales / combate
--      5 = Dance        → Baile
--      6 = Aquatic      → Acuático
--      7 = Mind         → Mente y cuerpo (Yoga, Pilates…)
--
--  DayOfWeek (int): 0 = Domingo … 6 = Sábado  (convención .NET DayOfWeek)
-- =============================================================================

USE [GymGoDb_Dev];
GO

-- ─────────────────────────────────────────────────────────────────────────────
--  TABLA: GymClasses
-- ─────────────────────────────────────────────────────────────────────────────
IF OBJECT_ID(N'[dbo].[GymClasses]', N'U') IS NOT NULL
BEGIN
    PRINT 'La tabla [dbo].[GymClasses] ya existe. Sin cambios.';
    -- No hacemos RETURN aquí para que se pueda crear ClassSchedules si falta.
END
ELSE
BEGIN

CREATE TABLE [dbo].[GymClasses]
(
    -- ── Identidad ─────────────────────────────────────────────────────────
    [Id]               UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_GymClasses] PRIMARY KEY,
    [TenantId]         UNIQUEIDENTIFIER NOT NULL,

    -- ── Datos de la clase ──────────────────────────────────────────────────
    [Name]             NVARCHAR(100)    NOT NULL,
    [Description]      NVARCHAR(500)    NULL,
    [Category]         INT              NOT NULL CONSTRAINT [DF_GymClasses_Category]    DEFAULT (0),
    [Color]            NVARCHAR(7)      NULL,   -- Hex color p.ej. '#3B82F6' para calendario
    [DurationMinutes]  INT              NOT NULL CONSTRAINT [DF_GymClasses_Duration]    DEFAULT (60),
    [MaxCapacity]      INT              NOT NULL CONSTRAINT [DF_GymClasses_MaxCapacity] DEFAULT (20),

    -- ── Estado ────────────────────────────────────────────────────────────
    [IsActive]         BIT              NOT NULL CONSTRAINT [DF_GymClasses_IsActive]    DEFAULT (1),

    -- ── IAuditable ────────────────────────────────────────────────────────
    [CreatedAtUtc]     DATETIME2(3)     NOT NULL,
    [CreatedBy]        NVARCHAR(200)    NULL,
    [ModifiedAtUtc]    DATETIME2(3)     NULL,
    [ModifiedBy]       NVARCHAR(200)    NULL,

    -- ── ISoftDeletable ────────────────────────────────────────────────────
    [IsDeleted]        BIT              NOT NULL CONSTRAINT [DF_GymClasses_IsDeleted]   DEFAULT (0),
    [DeletedAtUtc]     DATETIME2(3)     NULL,
    [DeletedBy]        NVARCHAR(200)    NULL,

    -- ── FK físicas ────────────────────────────────────────────────────────
    CONSTRAINT [FK_GymClasses_Tenants]
        FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants] ([Id]),

    -- ── CHECK constraints ─────────────────────────────────────────────────
    CONSTRAINT [CK_GymClasses_Category]         CHECK ([Category]        BETWEEN 0 AND 7),
    CONSTRAINT [CK_GymClasses_DurationMinutes]  CHECK ([DurationMinutes] > 0),
    CONSTRAINT [CK_GymClasses_MaxCapacity]      CHECK ([MaxCapacity]     > 0)
);

-- Índice por TenantId
CREATE NONCLUSTERED INDEX [IX_GymClasses_TenantId]
    ON [dbo].[GymClasses] ([TenantId] ASC);

-- Índice para listar activos del tenant
CREATE NONCLUSTERED INDEX [IX_GymClasses_TenantId_IsActive]
    ON [dbo].[GymClasses] ([TenantId] ASC, [IsActive] ASC)
    WHERE [IsDeleted] = 0;

PRINT 'Tabla [dbo].[GymClasses] creada.';

END
GO

-- ─────────────────────────────────────────────────────────────────────────────
--  TABLA: ClassSchedules
-- ─────────────────────────────────────────────────────────────────────────────
IF OBJECT_ID(N'[dbo].[ClassSchedules]', N'U') IS NOT NULL
BEGIN
    PRINT 'La tabla [dbo].[ClassSchedules] ya existe. Sin cambios.';
    RETURN;
END
GO

CREATE TABLE [dbo].[ClassSchedules]
(
    -- ── Identidad ─────────────────────────────────────────────────────────
    [Id]               UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_ClassSchedules] PRIMARY KEY,
    [TenantId]         UNIQUEIDENTIFIER NOT NULL,
    [GymClassId]       UNIQUEIDENTIFIER NOT NULL,

    -- ── Horario semanal recurrente ─────────────────────────────────────────
    -- DayOfWeek: 0=Domingo, 1=Lunes … 6=Sábado (convención .NET DayOfWeek)
    [DayOfWeek]        INT              NOT NULL,
    [StartTime]        TIME(0)          NOT NULL,   -- Hora de inicio (sin segundos)
    [EndTime]          TIME(0)          NOT NULL,   -- Hora de término calculada

    -- ── Datos operacionales ───────────────────────────────────────────────
    [InstructorName]   NVARCHAR(100)    NULL,
    [Room]             NVARCHAR(100)    NULL,
    -- Capacidad máxima para este horario (NULL = usa la de GymClass.MaxCapacity)
    [MaxCapacity]      INT              NULL,

    -- ── Estado ────────────────────────────────────────────────────────────
    [IsActive]         BIT              NOT NULL CONSTRAINT [DF_ClassSchedules_IsActive]  DEFAULT (1),

    -- ── IAuditable ────────────────────────────────────────────────────────
    [CreatedAtUtc]     DATETIME2(3)     NOT NULL,
    [CreatedBy]        NVARCHAR(200)    NULL,
    [ModifiedAtUtc]    DATETIME2(3)     NULL,
    [ModifiedBy]       NVARCHAR(200)    NULL,

    -- ── ISoftDeletable ────────────────────────────────────────────────────
    [IsDeleted]        BIT              NOT NULL CONSTRAINT [DF_ClassSchedules_IsDeleted] DEFAULT (0),
    [DeletedAtUtc]     DATETIME2(3)     NULL,
    [DeletedBy]        NVARCHAR(200)    NULL,

    -- ── FK físicas ────────────────────────────────────────────────────────
    CONSTRAINT [FK_ClassSchedules_Tenants]
        FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants] ([Id]),

    CONSTRAINT [FK_ClassSchedules_GymClasses]
        FOREIGN KEY ([GymClassId]) REFERENCES [dbo].[GymClasses] ([Id]),

    -- ── CHECK constraints ─────────────────────────────────────────────────
    CONSTRAINT [CK_ClassSchedules_DayOfWeek]    CHECK ([DayOfWeek]    BETWEEN 0 AND 6),
    CONSTRAINT [CK_ClassSchedules_TimeRange]    CHECK ([EndTime]      > [StartTime]),
    CONSTRAINT [CK_ClassSchedules_MaxCapacity]  CHECK ([MaxCapacity]  IS NULL OR [MaxCapacity] > 0)
);
GO

-- Índice por TenantId
CREATE NONCLUSTERED INDEX [IX_ClassSchedules_TenantId]
    ON [dbo].[ClassSchedules] ([TenantId] ASC);
GO

-- Índice por GymClassId (para listar horarios de una clase)
CREATE NONCLUSTERED INDEX [IX_ClassSchedules_GymClassId]
    ON [dbo].[ClassSchedules] ([GymClassId] ASC);
GO

-- Índice compuesto para calendario semanal del tenant
CREATE NONCLUSTERED INDEX [IX_ClassSchedules_TenantId_DayOfWeek]
    ON [dbo].[ClassSchedules] ([TenantId] ASC, [DayOfWeek] ASC, [StartTime] ASC)
    WHERE [IsDeleted] = 0 AND [IsActive] = 1;
GO

PRINT 'Tabla [dbo].[ClassSchedules] creada.';
GO

-- =============================================================================
--  GymGo  ·  01_schema / 07_ClassAttendances.sql
--  Registro de asistencia de socios a sesiones de clases.
--
--  ClassAttendances  → un registro por cada check-in de un socio a una sesión.
--
--  CheckInMethod (int):
--      0 = Manual   → La recepcionista lo registró manualmente por nombre/RUT.
--      1 = QR       → El socio escaneó su código QR en recepción.
--
--  Restricción de unicidad:
--      Un mismo socio no puede tener dos check-ins para el mismo horario
--      en la misma fecha (MemberId + ClassScheduleId + SessionDate = único).
-- =============================================================================

USE [GymGoDb_Dev];
GO

IF OBJECT_ID(N'[dbo].[ClassAttendances]', N'U') IS NOT NULL
BEGIN
    PRINT 'La tabla [dbo].[ClassAttendances] ya existe. Sin cambios.';
    RETURN;
END
GO

CREATE TABLE [dbo].[ClassAttendances]
(
    -- ── Identidad ─────────────────────────────────────────────────────────
    [Id]               UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_ClassAttendances] PRIMARY KEY,
    [TenantId]         UNIQUEIDENTIFIER NOT NULL,

    -- ── Relaciones ────────────────────────────────────────────────────────
    [MemberId]         UNIQUEIDENTIFIER NOT NULL,
    [ClassScheduleId]  UNIQUEIDENTIFIER NOT NULL,

    -- ── Datos de la sesión ────────────────────────────────────────────────
    -- Fecha concreta del día en que ocurrió la clase (sin hora, en UTC).
    -- Junto con ClassScheduleId identifica de forma única la sesión.
    [SessionDate]      DATE             NOT NULL,

    -- Timestamp exacto del check-in (UTC).
    [CheckedInAtUtc]   DATETIME2(3)     NOT NULL,

    -- Método de check-in: 0 = Manual, 1 = QR.
    [CheckInMethod]    INT              NOT NULL CONSTRAINT [DF_ClassAttendances_Method] DEFAULT (0),

    -- Nombre completo del socio en el momento del check-in (snapshot).
    -- Evita un JOIN para mostrar el historial si el socio cambia de nombre.
    [MemberFullName]   NVARCHAR(200)    NOT NULL,

    -- Notas opcionales de la recepcionista.
    [Notes]            NVARCHAR(500)    NULL,

    -- ── IAuditable ────────────────────────────────────────────────────────
    [CreatedAtUtc]     DATETIME2(3)     NOT NULL,
    [CreatedBy]        NVARCHAR(200)    NULL,
    [ModifiedAtUtc]    DATETIME2(3)     NULL,
    [ModifiedBy]       NVARCHAR(200)    NULL,

    -- ── FK físicas ────────────────────────────────────────────────────────
    CONSTRAINT [FK_ClassAttendances_Tenants]
        FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants] ([Id]),

    CONSTRAINT [FK_ClassAttendances_Members]
        FOREIGN KEY ([MemberId]) REFERENCES [dbo].[Members] ([Id]),

    CONSTRAINT [FK_ClassAttendances_ClassSchedules]
        FOREIGN KEY ([ClassScheduleId]) REFERENCES [dbo].[ClassSchedules] ([Id]),

    -- ── CHECK constraints ─────────────────────────────────────────────────
    CONSTRAINT [CK_ClassAttendances_CheckInMethod]
        CHECK ([CheckInMethod] IN (0, 1)),

    -- ── Unicidad: un socio solo puede hacer check-in una vez por sesión ───
    CONSTRAINT [UQ_ClassAttendances_Member_Schedule_Date]
        UNIQUE ([MemberId], [ClassScheduleId], [SessionDate])
);
GO

-- Índice principal de consulta: asistencias de una sesión (horario + fecha)
CREATE NONCLUSTERED INDEX [IX_ClassAttendances_Schedule_Date]
    ON [dbo].[ClassAttendances] ([TenantId] ASC, [ClassScheduleId] ASC, [SessionDate] ASC);
GO

-- Índice para historial de un socio
CREATE NONCLUSTERED INDEX [IX_ClassAttendances_Member]
    ON [dbo].[ClassAttendances] ([TenantId] ASC, [MemberId] ASC, [SessionDate] DESC);
GO

PRINT 'Tabla [dbo].[ClassAttendances] creada.';
GO

-- =============================================================================
--  GymGo  ·  01_schema / 08_GymEntries.sql
--  Registro de ingresos generales al gimnasio (acceso a las instalaciones).
--
--  Se crea un registro únicamente cuando el acceso es aprobado:
--    - Socio con estado Active.
--    - Asignación de membresía con Status = Active y EndDate >= hoy.
--    - Día habilitado por el plan (si FixedDays = true).
--    - Horario dentro del rango permitido (si FreeSchedule = false).
--
--  GymEntryMethod (enum, int):
--      0 = Manual   → Registro manual por recepcionista
--      1 = QR       → Código QR escaneado por el socio
--      2 = Badge    → Tarjeta o llavero RFID/NFC
--
--  Notas de diseño:
--    - No tiene soft delete: los ingresos son inmutables por auditoría.
--    - MemberFullName es un snapshot del nombre al momento del ingreso.
-- =============================================================================

USE [GymGoDb_Dev];
GO

IF OBJECT_ID(N'[dbo].[GymEntries]', N'U') IS NOT NULL
BEGIN
    PRINT 'La tabla [dbo].[GymEntries] ya existe. Sin cambios.';
    RETURN;
END
GO

CREATE TABLE [dbo].[GymEntries]
(
    -- ── Identidad ────────────────────────────────────────────────────────
    [Id]                      UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_GymEntries] PRIMARY KEY,
    [TenantId]                UNIQUEIDENTIFIER NOT NULL,

    -- ── Relaciones ───────────────────────────────────────────────────────
    [MemberId]                UNIQUEIDENTIFIER NOT NULL,
    [MembershipAssignmentId]  UNIQUEIDENTIFIER NOT NULL,

    -- ── Datos del ingreso ─────────────────────────────────────────────────
    [EntryDate]               DATE             NOT NULL,
    [EnteredAtUtc]            DATETIME2(3)     NOT NULL,
    [Method]                  INT              NOT NULL CONSTRAINT [DF_GE_Method] DEFAULT (0),

    -- Snapshot del nombre del socio al momento del ingreso
    [MemberFullName]          NVARCHAR(200)    NOT NULL,
    [Notes]                   NVARCHAR(500)    NULL,

    -- ── IAuditable ───────────────────────────────────────────────────────
    [CreatedAtUtc]            DATETIME2(3)     NOT NULL,
    [CreatedBy]               NVARCHAR(200)    NULL,
    [ModifiedAtUtc]           DATETIME2(3)     NULL,
    [ModifiedBy]              NVARCHAR(200)    NULL,

    -- ── FK físicas ────────────────────────────────────────────────────────
    CONSTRAINT [FK_GymEntries_Members]
        FOREIGN KEY ([MemberId]) REFERENCES [dbo].[Members] ([Id]),

    CONSTRAINT [FK_GymEntries_MembershipAssignments]
        FOREIGN KEY ([MembershipAssignmentId]) REFERENCES [dbo].[MembershipAssignments] ([Id]),

    CONSTRAINT [FK_GymEntries_Tenants]
        FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants] ([Id]),

    -- ── CHECK constraints ─────────────────────────────────────────────────
    CONSTRAINT [CK_GE_Method] CHECK ([Method] BETWEEN 0 AND 2)
);
GO

-- Historial de ingresos de un socio (consulta frecuente desde perfil del socio)
CREATE NONCLUSTERED INDEX [IX_GymEntries_Member_Date]
    ON [dbo].[GymEntries] ([TenantId] ASC, [MemberId] ASC, [EntryDate] DESC);
GO

-- Listado de ingresos del día en recepción (consulta en tiempo real)
CREATE NONCLUSTERED INDEX [IX_GymEntries_Tenant_Date]
    ON [dbo].[GymEntries] ([TenantId] ASC, [EntryDate] DESC);
GO

PRINT 'Tabla [dbo].[GymEntries] creada.';
GO

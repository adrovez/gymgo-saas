-- =============================================================================
--  GymGo  ·  01_schema / 04_MembershipPlans.sql
--  Planes de membresía que cada gimnasio ofrece a sus socios.
--
--  Periodicidad (Periodicity enum, persistido como int):
--      1 = Monthly     → Mensual   (30 días)
--      2 = Quarterly   → Trimestral (90 días)
--      3 = Biannual    → Semestral  (180 días)
--      4 = Annual      → Anual      (365 días)
-- =============================================================================

USE [GymGoDb_Dev];
GO

IF OBJECT_ID(N'[dbo].[MembershipPlans]', N'U') IS NOT NULL
BEGIN
    PRINT 'La tabla [dbo].[MembershipPlans] ya existe. Sin cambios.';
    RETURN;
END
GO

CREATE TABLE [dbo].[MembershipPlans]
(
    -- ── Identidad ────────────────────────────────────────────────────────
    [Id]               UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_MembershipPlans] PRIMARY KEY,
    [TenantId]         UNIQUEIDENTIFIER NOT NULL,

    -- ── Descripción comercial ────────────────────────────────────────────
    [Name]             NVARCHAR(150)    NOT NULL,
    [Description]      NVARCHAR(500)    NULL,

    -- ── Periodicidad y duración ──────────────────────────────────────────
    [Periodicity]      INT              NOT NULL,
    -- DurationDays derivado de Periodicity: 30 / 90 / 180 / 365
    [DurationDays]     INT              NOT NULL,

    -- ── Asistencia ───────────────────────────────────────────────────────
    -- Número referencial de días por semana (1-7)
    [DaysPerWeek]      INT              NOT NULL,
    -- Si true: los días de la semana son fijos (ver columnas Monday-Sunday)
    -- Si false: el socio elige qué días dentro del límite de DaysPerWeek
    [FixedDays]        BIT              NOT NULL CONSTRAINT [DF_MP_FixedDays]   DEFAULT (0),
    [Monday]           BIT              NOT NULL CONSTRAINT [DF_MP_Monday]      DEFAULT (0),
    [Tuesday]          BIT              NOT NULL CONSTRAINT [DF_MP_Tuesday]     DEFAULT (0),
    [Wednesday]        BIT              NOT NULL CONSTRAINT [DF_MP_Wednesday]   DEFAULT (0),
    [Thursday]         BIT              NOT NULL CONSTRAINT [DF_MP_Thursday]    DEFAULT (0),
    [Friday]           BIT              NOT NULL CONSTRAINT [DF_MP_Friday]      DEFAULT (0),
    [Saturday]         BIT              NOT NULL CONSTRAINT [DF_MP_Saturday]    DEFAULT (0),
    [Sunday]           BIT              NOT NULL CONSTRAINT [DF_MP_Sunday]      DEFAULT (0),

    -- ── Horario ──────────────────────────────────────────────────────────
    -- Si true: acceso en cualquier horario
    -- Si false: acceso sólo entre TimeFrom y TimeTo
    [FreeSchedule]     BIT              NOT NULL CONSTRAINT [DF_MP_FreeSchedule] DEFAULT (1),
    [TimeFrom]         TIME             NULL,
    [TimeTo]           TIME             NULL,

    -- ── Comercial ────────────────────────────────────────────────────────
    -- Monto total del plan en CLP (no mensual)
    [Amount]           DECIMAL(18,2)    NOT NULL,
    [AllowsFreezing]   BIT              NOT NULL CONSTRAINT [DF_MP_AllowsFreezing] DEFAULT (0),

    -- ── Ciclo de vida del plan ────────────────────────────────────────────
    [IsActive]         BIT              NOT NULL CONSTRAINT [DF_MP_IsActive]    DEFAULT (1),
    [DeactivatedAtUtc] DATETIME2(3)     NULL,

    -- ── IAuditable ───────────────────────────────────────────────────────
    [CreatedAtUtc]     DATETIME2(3)     NOT NULL,
    [CreatedBy]        NVARCHAR(200)    NULL,
    [ModifiedAtUtc]    DATETIME2(3)     NULL,
    [ModifiedBy]       NVARCHAR(200)    NULL,

    -- ── ISoftDeletable ───────────────────────────────────────────────────
    [IsDeleted]        BIT              NOT NULL CONSTRAINT [DF_MP_IsDeleted]   DEFAULT (0),
    [DeletedAtUtc]     DATETIME2(3)     NULL,
    [DeletedBy]        NVARCHAR(200)    NULL,

    -- ── CHECK constraints ─────────────────────────────────────────────────
    CONSTRAINT [CK_MP_Periodicity]  CHECK ([Periodicity]  BETWEEN 1 AND 4),
    CONSTRAINT [CK_MP_DaysPerWeek]  CHECK ([DaysPerWeek]  BETWEEN 1 AND 7),
    CONSTRAINT [CK_MP_DurationDays] CHECK ([DurationDays] > 0),
    CONSTRAINT [CK_MP_Amount]       CHECK ([Amount] > 0),
    -- Si horario no es libre, ambas horas deben estar presentes y TimeFrom < TimeTo
    CONSTRAINT [CK_MP_TimeRange]    CHECK (
        [FreeSchedule] = 1
        OR ([TimeFrom] IS NOT NULL AND [TimeTo] IS NOT NULL AND [TimeFrom] < [TimeTo])
    )
);
GO

-- Índice por TenantId para filtrado por tenant.
CREATE NONCLUSTERED INDEX [IX_MembershipPlans_TenantId]
    ON [dbo].[MembershipPlans] ([TenantId] ASC);
GO

-- Índice para queries de planes activos por tenant (caso más frecuente).
CREATE NONCLUSTERED INDEX [IX_MembershipPlans_TenantId_IsActive]
    ON [dbo].[MembershipPlans] ([TenantId] ASC, [IsActive] ASC)
    WHERE [IsDeleted] = 0;
GO

PRINT 'Tabla [dbo].[MembershipPlans] creada.';
GO

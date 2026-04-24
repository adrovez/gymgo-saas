-- =============================================================================
--  GymGo  ·  01_schema / 05_MembershipAssignments.sql
--  Asignaciones de planes de membresía a socios.
--  Registra el contrato vigente e histórico entre un socio y su plan.
--
--  AssignmentStatus (enum, int):
--      0 = Active      → Vigente
--      1 = Expired     → Vencida
--      2 = Cancelled   → Cancelada manualmente
--      3 = Frozen      → Congelada (pausada)
--
--  PaymentStatus (enum, int):
--      0 = Pending     → Pago no registrado
--      1 = Paid        → Pago confirmado
--      2 = Overdue     → Morosa (venció sin pago)
-- =============================================================================

USE [GymGoDb_Dev];
GO

IF OBJECT_ID(N'[dbo].[MembershipAssignments]', N'U') IS NOT NULL
BEGIN
    PRINT 'La tabla [dbo].[MembershipAssignments] ya existe. Sin cambios.';
    RETURN;
END
GO

CREATE TABLE [dbo].[MembershipAssignments]
(
    -- ── Identidad ────────────────────────────────────────────────────────
    [Id]                    UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_MembershipAssignments] PRIMARY KEY,
    [TenantId]              UNIQUEIDENTIFIER NOT NULL,
    [MemberId]              UNIQUEIDENTIFIER NOT NULL,
    [MembershipPlanId]      UNIQUEIDENTIFIER NOT NULL,

    -- ── Período ──────────────────────────────────────────────────────────
    [StartDate]             DATE             NOT NULL,
    [EndDate]               DATE             NOT NULL,

    -- ── Snapshot comercial ────────────────────────────────────────────────
    -- Monto al momento de la asignación. Inmutable aunque el plan cambie de precio.
    [AmountSnapshot]        DECIMAL(18,2)    NOT NULL,

    -- ── Estado ───────────────────────────────────────────────────────────
    [Status]                INT              NOT NULL CONSTRAINT [DF_MA_Status]        DEFAULT (0),
    [PaymentStatus]         INT              NOT NULL CONSTRAINT [DF_MA_PaymentStatus] DEFAULT (0),
    [PaidAtUtc]             DATETIME2(3)     NULL,

    -- ── Congelamiento ─────────────────────────────────────────────────────
    -- Fecha desde la que está congelada (null si no está congelada)
    [FrozenSince]           DATE             NULL,
    -- Días acumulados de congelamiento (sumados a EndDate al descongelar)
    [FrozenDaysAccumulated] INT              NOT NULL CONSTRAINT [DF_MA_FrozenDays]    DEFAULT (0),

    -- ── Observaciones ─────────────────────────────────────────────────────
    [Notes]                 NVARCHAR(500)    NULL,

    -- ── IAuditable ───────────────────────────────────────────────────────
    [CreatedAtUtc]          DATETIME2(3)     NOT NULL,
    [CreatedBy]             NVARCHAR(200)    NULL,
    [ModifiedAtUtc]         DATETIME2(3)     NULL,
    [ModifiedBy]            NVARCHAR(200)    NULL,

    -- ── ISoftDeletable ───────────────────────────────────────────────────
    [IsDeleted]             BIT              NOT NULL CONSTRAINT [DF_MA_IsDeleted]     DEFAULT (0),
    [DeletedAtUtc]          DATETIME2(3)     NULL,
    [DeletedBy]             NVARCHAR(200)    NULL,

    -- ── FK físicas ────────────────────────────────────────────────────────
    CONSTRAINT [FK_MembershipAssignments_Members]
        FOREIGN KEY ([MemberId]) REFERENCES [dbo].[Members] ([Id]),

    CONSTRAINT [FK_MembershipAssignments_MembershipPlans]
        FOREIGN KEY ([MembershipPlanId]) REFERENCES [dbo].[MembershipPlans] ([Id]),

    CONSTRAINT [FK_MembershipAssignments_Tenants]
        FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants] ([Id]),

    -- ── CHECK constraints ─────────────────────────────────────────────────
    CONSTRAINT [CK_MA_Status]          CHECK ([Status]        BETWEEN 0 AND 3),
    CONSTRAINT [CK_MA_PaymentStatus]   CHECK ([PaymentStatus] BETWEEN 0 AND 2),
    CONSTRAINT [CK_MA_AmountSnapshot]  CHECK ([AmountSnapshot] > 0),
    CONSTRAINT [CK_MA_DateRange]       CHECK ([EndDate] > [StartDate]),
    CONSTRAINT [CK_MA_FrozenDays]      CHECK ([FrozenDaysAccumulated] >= 0)
);
GO

-- Índice por TenantId
CREATE NONCLUSTERED INDEX [IX_MembershipAssignments_TenantId]
    ON [dbo].[MembershipAssignments] ([TenantId] ASC);
GO

-- Índice por MemberId para historial rápido por socio
CREATE NONCLUSTERED INDEX [IX_MembershipAssignments_MemberId]
    ON [dbo].[MembershipAssignments] ([MemberId] ASC);
GO

-- Índice compuesto para buscar la asignación activa de un socio (query más frecuente)
CREATE NONCLUSTERED INDEX [IX_MembershipAssignments_MemberId_Status]
    ON [dbo].[MembershipAssignments] ([MemberId] ASC, [Status] ASC)
    WHERE [IsDeleted] = 0;
GO

-- Índice para listado de morosos del tenant
CREATE NONCLUSTERED INDEX [IX_MembershipAssignments_TenantId_PaymentStatus]
    ON [dbo].[MembershipAssignments] ([TenantId] ASC, [PaymentStatus] ASC)
    WHERE [IsDeleted] = 0;
GO

PRINT 'Tabla [dbo].[MembershipAssignments] creada.';
GO

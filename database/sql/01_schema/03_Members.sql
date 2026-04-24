-- =============================================================================
--  GymGo  ·  01_schema / 03_Members.sql
--  Socios del gimnasio (Members).
--  Un socio pertenece a un único Tenant y puede tener una cuenta User asociada
--  (rol Member) para acceso a la app móvil, pero esa relación es opcional.
--
--  Estado (MemberStatus enum, persistido como int):
--      0 = Active      → Socio activo con membresía vigente
--      1 = Suspended   → Suspendido manualmente por el staff
--      2 = Delinquent  → Moroso (cuotas impagas)
--
--  Género (Gender enum, persistido como int):
--      0 = NotSpecified
--      1 = Male
--      2 = Female
--      3 = Other
-- =============================================================================

USE [GymGoDb_Dev];
GO

IF OBJECT_ID(N'[dbo].[Members]', N'U') IS NOT NULL
BEGIN
    PRINT 'La tabla [dbo].[Members] ya existe. Sin cambios.';
    RETURN;
END
GO

CREATE TABLE [dbo].[Members]
(
    -- ── Identificación ──────────────────────────────────────────────────
    [Id]                     UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_Members] PRIMARY KEY,
    [TenantId]               UNIQUEIDENTIFIER NOT NULL,

    -- RUT chileno normalizado: sin puntos, con guión. Ej: "12345678-9"
    [Rut]                    NVARCHAR(20)     NOT NULL,
    [FirstName]              NVARCHAR(100)    NOT NULL,
    [LastName]               NVARCHAR(100)    NOT NULL,
    [BirthDate]              DATE             NOT NULL,
    [Gender]                 INT              NOT NULL CONSTRAINT [DF_Members_Gender]  DEFAULT (0),

    -- ── Contacto ────────────────────────────────────────────────────────
    [Email]                  NVARCHAR(200)    NULL,
    [Phone]                  NVARCHAR(40)     NULL,
    [Address]                NVARCHAR(300)    NULL,

    -- ── Contacto de emergencia ───────────────────────────────────────────
    [EmergencyContactName]   NVARCHAR(200)    NULL,
    [EmergencyContactPhone]  NVARCHAR(40)     NULL,

    -- ── Estado y membresía ───────────────────────────────────────────────
    [Status]                 INT              NOT NULL CONSTRAINT [DF_Members_Status]  DEFAULT (0),
    -- Fecha en que el socio se unió al gimnasio (distinta del CreatedAtUtc técnico)
    [RegistrationDate]       DATE             NOT NULL,

    -- ── Observaciones ────────────────────────────────────────────────────
    [Notes]                  NVARCHAR(1000)   NULL,

    -- ── IAuditable ───────────────────────────────────────────────────────
    [CreatedAtUtc]           DATETIME2(3)     NOT NULL,
    [CreatedBy]              NVARCHAR(200)    NULL,
    [ModifiedAtUtc]          DATETIME2(3)     NULL,
    [ModifiedBy]             NVARCHAR(200)    NULL,

    -- ── ISoftDeletable ───────────────────────────────────────────────────
    [IsDeleted]              BIT              NOT NULL CONSTRAINT [DF_Members_IsDeleted] DEFAULT (0),
    [DeletedAtUtc]           DATETIME2(3)     NULL,
    [DeletedBy]              NVARCHAR(200)    NULL,

    -- ── CHECK constraints ────────────────────────────────────────────────
    CONSTRAINT [CK_Members_Status] CHECK ([Status] BETWEEN 0 AND 2),
    CONSTRAINT [CK_Members_Gender] CHECK ([Gender] BETWEEN 0 AND 3)
);
GO

-- RUT único por tenant entre registros no eliminados.
-- Un mismo RUT puede re-darse de alta después de un soft-delete.
CREATE UNIQUE NONCLUSTERED INDEX [UX_Members_TenantId_Rut]
    ON [dbo].[Members] ([TenantId] ASC, [Rut] ASC)
    WHERE [IsDeleted] = 0;
GO

-- Índice secundario por TenantId para queries filtradas por tenant.
CREATE NONCLUSTERED INDEX [IX_Members_TenantId]
    ON [dbo].[Members] ([TenantId] ASC);
GO

-- Índice para búsquedas frecuentes por estado dentro de un tenant.
CREATE NONCLUSTERED INDEX [IX_Members_TenantId_Status]
    ON [dbo].[Members] ([TenantId] ASC, [Status] ASC)
    WHERE [IsDeleted] = 0;
GO

PRINT 'Tabla [dbo].[Members] creada.';
GO

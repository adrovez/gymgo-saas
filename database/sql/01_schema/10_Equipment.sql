-- =============================================================================
--  GymGo  ·  01_schema / 10_Equipment.sql
--  Catálogo de maquinaria del gimnasio.
--
--  Equipment → cada máquina o equipo físico del gimnasio.
--              Se relaciona con MaintenanceRecords (1:N).
-- =============================================================================

USE [GymGoDb_Dev];
GO

IF OBJECT_ID(N'[dbo].[Equipment]', N'U') IS NOT NULL
BEGIN
    PRINT 'La tabla [dbo].[Equipment] ya existe. Sin cambios.';
    RETURN;
END
GO

CREATE TABLE [dbo].[Equipment]
(
    -- ── Identidad ─────────────────────────────────────────────────────────
    [Id]              UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_Equipment] PRIMARY KEY,
    [TenantId]        UNIQUEIDENTIFIER NOT NULL,

    -- ── Datos de la máquina ───────────────────────────────────────────────
    [Name]            NVARCHAR(100)    NOT NULL,
    [Brand]           NVARCHAR(100)    NULL,
    [Model]           NVARCHAR(100)    NULL,
    [SerialNumber]    NVARCHAR(50)     NULL,
    [PurchaseDate]    DATE             NULL,
    [ImageUrl]        NVARCHAR(500)    NULL,

    -- ── Estado ────────────────────────────────────────────────────────────
    [IsActive]        BIT              NOT NULL CONSTRAINT [DF_Equipment_IsActive]   DEFAULT (1),

    -- ── IAuditable ────────────────────────────────────────────────────────
    [CreatedAtUtc]    DATETIME2(3)     NOT NULL,
    [CreatedBy]       NVARCHAR(200)    NULL,
    [ModifiedAtUtc]   DATETIME2(3)     NULL,
    [ModifiedBy]      NVARCHAR(200)    NULL,

    -- ── ISoftDeletable ────────────────────────────────────────────────────
    [IsDeleted]       BIT              NOT NULL CONSTRAINT [DF_Equipment_IsDeleted]  DEFAULT (0),
    [DeletedAtUtc]    DATETIME2(3)     NULL,
    [DeletedBy]       NVARCHAR(200)    NULL,

    -- ── FK físicas ────────────────────────────────────────────────────────
    CONSTRAINT [FK_Equipment_Tenants]
        FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants] ([Id])
);
GO

-- Índice por TenantId
CREATE NONCLUSTERED INDEX [IX_Equipment_TenantId]
    ON [dbo].[Equipment] ([TenantId] ASC);
GO

-- Índice para listar activas del tenant (filtro más frecuente)
CREATE NONCLUSTERED INDEX [IX_Equipment_TenantId_IsActive]
    ON [dbo].[Equipment] ([TenantId] ASC, [IsActive] ASC)
    WHERE [IsDeleted] = 0;
GO

PRINT 'Tabla [dbo].[Equipment] creada.';
GO

-- =============================================================================
--  GymGo  ·  01_schema / 11_MaintenanceRecords.sql
--  Registro de mantenciones de maquinaria (preventivas y correctivas).
--
--  MaintenanceType (enum, int):
--      0 = Preventive   → Mantención programada / periódica
--      1 = Corrective   → Reparación ante falla o daño reportado
--
--  MaintenanceStatus (enum, int):
--      0 = Pending      → Registrada, aún no iniciada
--      1 = InProgress   → En ejecución
--      2 = Completed    → Finalizada con éxito
--      3 = Cancelled    → Cancelada antes de completarse
--
--  ResponsibleType (enum, int):
--      0 = Internal     → Ejecutada por staff interno del gimnasio
--      1 = External     → Ejecutada por proveedor o técnico externo
-- =============================================================================

USE [GymGoDb_Dev];
GO

IF OBJECT_ID(N'[dbo].[MaintenanceRecords]', N'U') IS NOT NULL
BEGIN
    PRINT 'La tabla [dbo].[MaintenanceRecords] ya existe. Sin cambios.';
    RETURN;
END
GO

CREATE TABLE [dbo].[MaintenanceRecords]
(
    -- ── Identidad ─────────────────────────────────────────────────────────
    [Id]                      UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_MaintenanceRecords] PRIMARY KEY,
    [TenantId]                UNIQUEIDENTIFIER NOT NULL,
    [EquipmentId]             UNIQUEIDENTIFIER NOT NULL,

    -- ── Tipo y estado ─────────────────────────────────────────────────────
    [Type]                    INT              NOT NULL,   -- MaintenanceType enum
    [Status]                  INT              NOT NULL CONSTRAINT [DF_MR_Status] DEFAULT (0),

    -- ── Fechas ────────────────────────────────────────────────────────────
    [ScheduledDate]           DATE             NOT NULL,   -- Fecha programada de la mantención
    [StartedAtUtc]            DATETIME2(3)     NULL,       -- Momento real de inicio
    [CompletedAtUtc]          DATETIME2(3)     NULL,       -- Momento real de cierre

    -- ── Descripción ───────────────────────────────────────────────────────
    [Description]             NVARCHAR(500)    NOT NULL,   -- Qué se va a hacer / qué se hizo
    [Notes]                   NVARCHAR(1000)   NULL,       -- Observaciones, motivo de cancelación, etc.
    [Cost]                    DECIMAL(10, 2)   NULL,       -- Costo incurrido (se carga al completar)

    -- ── Responsable ───────────────────────────────────────────────────────
    [ResponsibleType]         INT              NOT NULL,   -- ResponsibleType enum
    [ResponsibleUserId]       UNIQUEIDENTIFIER NULL,       -- FK → Users (si es interno)
    [ExternalProviderName]    NVARCHAR(200)    NULL,       -- Nombre del proveedor externo
    [ExternalProviderContact] NVARCHAR(200)    NULL,       -- Teléfono / email del proveedor

    -- ── IAuditable ────────────────────────────────────────────────────────
    [CreatedAtUtc]            DATETIME2(3)     NOT NULL,
    [CreatedBy]               NVARCHAR(200)    NULL,
    [ModifiedAtUtc]           DATETIME2(3)     NULL,
    [ModifiedBy]              NVARCHAR(200)    NULL,

    -- ── FK físicas ────────────────────────────────────────────────────────
    CONSTRAINT [FK_MaintenanceRecords_Tenants]
        FOREIGN KEY ([TenantId])    REFERENCES [dbo].[Tenants]   ([Id]),

    CONSTRAINT [FK_MaintenanceRecords_Equipment]
        FOREIGN KEY ([EquipmentId]) REFERENCES [dbo].[Equipment] ([Id]),

    CONSTRAINT [FK_MaintenanceRecords_Users]
        FOREIGN KEY ([ResponsibleUserId]) REFERENCES [dbo].[Users] ([Id]),

    -- ── CHECK constraints ─────────────────────────────────────────────────
    CONSTRAINT [CK_MR_Type]            CHECK ([Type]            IN (0, 1)),
    CONSTRAINT [CK_MR_Status]          CHECK ([Status]          IN (0, 1, 2, 3)),
    CONSTRAINT [CK_MR_ResponsibleType] CHECK ([ResponsibleType] IN (0, 1)),
    CONSTRAINT [CK_MR_Cost]            CHECK ([Cost]            IS NULL OR [Cost] >= 0)
);
GO

-- Índice por TenantId
CREATE NONCLUSTERED INDEX [IX_MR_TenantId]
    ON [dbo].[MaintenanceRecords] ([TenantId] ASC);
GO

-- Índice por EquipmentId (historial de una máquina)
CREATE NONCLUSTERED INDEX [IX_MR_EquipmentId]
    ON [dbo].[MaintenanceRecords] ([EquipmentId] ASC);
GO

-- Índice compuesto para filtrar por tenant + estado + fecha programada
CREATE NONCLUSTERED INDEX [IX_MR_TenantId_Status_ScheduledDate]
    ON [dbo].[MaintenanceRecords] ([TenantId] ASC, [Status] ASC, [ScheduledDate] ASC);
GO

PRINT 'Tabla [dbo].[MaintenanceRecords] creada.';
GO

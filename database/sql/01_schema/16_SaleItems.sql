-- =============================================================================
--  GymGo  ·  01_schema / 16_SaleItems.sql
--  Líneas de detalle de una venta.
--
--  ItemType (enum, int):
--      0 = Plan           → Plan de membresía (máximo 1 por venta)
--      1 = Matricula      → Inscripción / matrícula
--      2 = ServicioExtra  → Servicio adicional (ej: evaluación física)
--      3 = Otro           → Concepto libre
-- =============================================================================

USE [GymGoDb_Dev];
GO

IF OBJECT_ID(N'[dbo].[SaleItems]', N'U') IS NOT NULL
BEGIN
    PRINT 'La tabla [dbo].[SaleItems] ya existe. Sin cambios.';
    RETURN;
END
GO

CREATE TABLE [dbo].[SaleItems]
(
    -- ── Identidad ────────────────────────────────────────────────────────
    [Id]                 UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_SaleItems] PRIMARY KEY,
    [TenantId]           UNIQUEIDENTIFIER NOT NULL,
    [SaleId]             UNIQUEIDENTIFIER NOT NULL,

    -- ── Tipo e identificación del ítem ───────────────────────────────────
    [ItemType]           INT              NOT NULL,
    [Description]        NVARCHAR(200)    NOT NULL,

    -- ── Referencia opcional al plan (solo ItemType = Plan) ───────────────
    [MembershipPlanId]   UNIQUEIDENTIFIER NULL,

    -- ── Precio ────────────────────────────────────────────────────────────
    [UnitPrice]          DECIMAL(18,2)    NOT NULL,
    [Quantity]           INT              NOT NULL CONSTRAINT [DF_SaleItems_Qty] DEFAULT (1),
    [Subtotal]           AS               ([UnitPrice] * [Quantity]) PERSISTED,

    -- ── FK físicas ────────────────────────────────────────────────────────
    CONSTRAINT [FK_SaleItems_Sales]
        FOREIGN KEY ([SaleId]) REFERENCES [dbo].[Sales] ([Id]) ON DELETE CASCADE,

    CONSTRAINT [FK_SaleItems_Tenants]
        FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants] ([Id]),

    CONSTRAINT [FK_SaleItems_MembershipPlans]
        FOREIGN KEY ([MembershipPlanId]) REFERENCES [dbo].[MembershipPlans] ([Id]),

    -- ── CHECK constraints ─────────────────────────────────────────────────
    CONSTRAINT [CK_SaleItems_ItemType]   CHECK ([ItemType]   IN (0, 1, 2, 3)),
    CONSTRAINT [CK_SaleItems_UnitPrice]  CHECK ([UnitPrice]  >  0),
    CONSTRAINT [CK_SaleItems_Quantity]   CHECK ([Quantity]   >= 1),

    -- Si ItemType = Plan (0), MembershipPlanId es obligatorio
    CONSTRAINT [CK_SaleItems_PlanRef]
        CHECK ([ItemType] <> 0 OR [MembershipPlanId] IS NOT NULL)
);
GO

CREATE NONCLUSTERED INDEX [IX_SaleItems_SaleId]
    ON [dbo].[SaleItems] ([SaleId] ASC);
GO

PRINT 'Tabla [dbo].[SaleItems] creada.';
GO

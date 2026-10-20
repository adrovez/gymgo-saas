-- =============================================================================
--  GymGo  ·  01_schema / 17_SaleDiscounts.sql
--  Descuentos aplicados a una venta.
--
--  DiscountType (enum, int):
--      0 = MontoFijo   → Amount = Value (monto absoluto en CLP)
--      1 = Porcentaje  → Amount = SubtotalAmount × Value / 100
--
--  Puede haber más de un descuento por venta.
--  La suma de todos los Amount de descuentos no puede superar el SubtotalAmount
--  de la venta (validado en dominio, no en SQL).
-- =============================================================================

USE [GymGoDb_Dev];
GO

IF OBJECT_ID(N'[dbo].[SaleDiscounts]', N'U') IS NOT NULL
BEGIN
    PRINT 'La tabla [dbo].[SaleDiscounts] ya existe. Sin cambios.';
    RETURN;
END
GO

CREATE TABLE [dbo].[SaleDiscounts]
(
    -- ── Identidad ────────────────────────────────────────────────────────
    [Id]            UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_SaleDiscounts] PRIMARY KEY,
    [TenantId]      UNIQUEIDENTIFIER NOT NULL,
    [SaleId]        UNIQUEIDENTIFIER NOT NULL,

    -- ── Tipo y descripción ────────────────────────────────────────────────
    [DiscountType]  INT              NOT NULL,
    [Description]   NVARCHAR(200)    NOT NULL,

    -- ── Valor y monto calculado ───────────────────────────────────────────
    -- Value: monto fijo en CLP o porcentaje (ej: 10.5 para 10,5%)
    [Value]         DECIMAL(18,2)    NOT NULL,
    -- Amount: monto calculado en CLP, siempre positivo. Se persiste para
    -- evitar recálculos y garantizar trazabilidad histórica.
    [Amount]        DECIMAL(18,2)    NOT NULL,

    -- ── FK físicas ────────────────────────────────────────────────────────
    CONSTRAINT [FK_SaleDiscounts_Sales]
        FOREIGN KEY ([SaleId]) REFERENCES [dbo].[Sales] ([Id]) ON DELETE CASCADE,

    CONSTRAINT [FK_SaleDiscounts_Tenants]
        FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants] ([Id]),

    -- ── CHECK constraints ─────────────────────────────────────────────────
    CONSTRAINT [CK_SaleDiscounts_Type]    CHECK ([DiscountType] IN (0, 1)),
    CONSTRAINT [CK_SaleDiscounts_Value]   CHECK ([Value]  > 0),
    CONSTRAINT [CK_SaleDiscounts_Amount]  CHECK ([Amount] > 0),

    -- Porcentaje no puede superar 100%
    CONSTRAINT [CK_SaleDiscounts_PctMax]
        CHECK ([DiscountType] <> 1 OR [Value] <= 100)
);
GO

CREATE NONCLUSTERED INDEX [IX_SaleDiscounts_SaleId]
    ON [dbo].[SaleDiscounts] ([SaleId] ASC);
GO

PRINT 'Tabla [dbo].[SaleDiscounts] creada.';
GO

-- =============================================================================
--  GymGo  ·  03_alterations / 02_CashTransactions_AddSaleId.sql
--  Agrega columna SaleId (nullable) a CashTransactions.
--
--  Columna nullable → compatibilidad total con registros existentes.
--  Las transacciones manuales o del flujo antiguo tienen SaleId = NULL.
--  Las transacciones generadas automáticamente por el módulo Sales
--  tendrán SaleId poblado para trazabilidad completa.
-- =============================================================================

USE [GymGoDb_Dev];
GO

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[CashTransactions]')
      AND name = 'SaleId'
)
BEGIN
    PRINT 'Columna SaleId ya existe en CashTransactions. Sin cambios.';
    RETURN;
END
GO

ALTER TABLE [dbo].[CashTransactions]
    ADD [SaleId] UNIQUEIDENTIFIER NULL;
GO

ALTER TABLE [dbo].[CashTransactions]
    ADD CONSTRAINT [FK_CashTransactions_Sales]
        FOREIGN KEY ([SaleId]) REFERENCES [dbo].[Sales] ([Id]);
GO

CREATE NONCLUSTERED INDEX [IX_CashTransactions_SaleId]
    ON [dbo].[CashTransactions] ([SaleId] ASC)
    WHERE [SaleId] IS NOT NULL;
GO

PRINT 'Columna SaleId agregada a CashTransactions.';
GO

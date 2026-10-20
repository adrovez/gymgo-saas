-- =============================================================================
--  GymGo  ·  03_alterations / 01_MembershipAssignments_AddSaleId.sql
--  Agrega columna SaleId (nullable) a MembershipAssignments.
--
--  Columna nullable → compatibilidad total con registros existentes.
--  Los registros históricos tienen SaleId = NULL (flujo antiguo).
--  Los nuevos registros creados a través del módulo Sales tendrán SaleId poblado.
-- =============================================================================

USE [GymGoDb_Dev];
GO

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[MembershipAssignments]')
      AND name = 'SaleId'
)
BEGIN
    PRINT 'Columna SaleId ya existe en MembershipAssignments. Sin cambios.';
    RETURN;
END
GO

ALTER TABLE [dbo].[MembershipAssignments]
    ADD [SaleId] UNIQUEIDENTIFIER NULL;
GO

ALTER TABLE [dbo].[MembershipAssignments]
    ADD CONSTRAINT [FK_MembershipAssignments_Sales]
        FOREIGN KEY ([SaleId]) REFERENCES [dbo].[Sales] ([Id]);
GO

CREATE NONCLUSTERED INDEX [IX_MembershipAssignments_SaleId]
    ON [dbo].[MembershipAssignments] ([SaleId] ASC)
    WHERE [SaleId] IS NOT NULL;
GO

PRINT 'Columna SaleId agregada a MembershipAssignments.';
GO

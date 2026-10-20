-- =============================================================================
--  GymGo  ·  01_schema / 15_Sales.sql
--  Cabecera de venta de servicios.
--  Una Sale agrupa todos los ítems cobrados al socio en una operación
--  y, al confirmarse, genera automáticamente la MembershipAssignment
--  y la CashTransaction correspondientes.
--
--  SaleStatus (enum, int):
--      0 = Draft       → En construcción, aún no cobrada
--      1 = Confirmed   → Cobrada — generó Assignment + CashTransaction
--      2 = Voided      → Anulada
--
--  PaymentMethod (enum, byte):
--      0 = Efectivo
--      1 = Tarjeta
--      2 = Transferencia
-- =============================================================================

USE [GymGoDb_Dev];
GO

IF OBJECT_ID(N'[dbo].[Sales]', N'U') IS NOT NULL
BEGIN
    PRINT 'La tabla [dbo].[Sales] ya existe. Sin cambios.';
    RETURN;
END
GO

CREATE TABLE [dbo].[Sales]
(
    -- ── Identidad ────────────────────────────────────────────────────────
    [Id]                      UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_Sales] PRIMARY KEY,
    [TenantId]                UNIQUEIDENTIFIER NOT NULL,

    -- ── Socio ─────────────────────────────────────────────────────────────
    [MemberId]                UNIQUEIDENTIFIER NOT NULL,

    -- ── Clasificación ─────────────────────────────────────────────────────
    [Date]                    DATE             NOT NULL,
    [Status]                  INT              NOT NULL CONSTRAINT [DF_Sales_Status]        DEFAULT (0),
    [PaymentMethod]           TINYINT          NOT NULL CONSTRAINT [DF_Sales_PaymentMethod] DEFAULT (0),

    -- ── Montos ────────────────────────────────────────────────────────────
    [SubtotalAmount]          DECIMAL(18,2)    NOT NULL CONSTRAINT [DF_Sales_Subtotal]      DEFAULT (0),
    [DiscountAmount]          DECIMAL(18,2)    NOT NULL CONSTRAINT [DF_Sales_Discount]      DEFAULT (0),
    [TotalAmount]             DECIMAL(18,2)    NOT NULL CONSTRAINT [DF_Sales_Total]         DEFAULT (0),

    -- ── Vínculos generados al confirmar ──────────────────────────────────
    [MembershipAssignmentId]  UNIQUEIDENTIFIER NULL,
    [CashTransactionId]       UNIQUEIDENTIFIER NULL,

    -- ── Observaciones ─────────────────────────────────────────────────────
    [Notes]                   NVARCHAR(500)    NULL,

    -- ── Auditoría ─────────────────────────────────────────────────────────
    [ProcessedByUserId]       UNIQUEIDENTIFIER NOT NULL,
    [CreatedAtUtc]            DATETIME2(3)     NOT NULL,
    [CreatedBy]               NVARCHAR(200)    NULL,
    [ModifiedAtUtc]           DATETIME2(3)     NULL,
    [ModifiedBy]              NVARCHAR(200)    NULL,

    -- ── Anulación ─────────────────────────────────────────────────────────
    [VoidedAtUtc]             DATETIME2(3)     NULL,
    [VoidReason]              NVARCHAR(500)    NULL,

    -- ── ISoftDeletable ───────────────────────────────────────────────────
    [IsDeleted]               BIT              NOT NULL CONSTRAINT [DF_Sales_IsDeleted]     DEFAULT (0),
    [DeletedAtUtc]            DATETIME2(3)     NULL,
    [DeletedBy]               NVARCHAR(200)    NULL,

    -- ── FK físicas ────────────────────────────────────────────────────────
    CONSTRAINT [FK_Sales_Tenants]
        FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants] ([Id]),

    CONSTRAINT [FK_Sales_Members]
        FOREIGN KEY ([MemberId]) REFERENCES [dbo].[Members] ([Id]),

    CONSTRAINT [FK_Sales_Users]
        FOREIGN KEY ([ProcessedByUserId]) REFERENCES [dbo].[Users] ([Id]),

    CONSTRAINT [FK_Sales_MembershipAssignments]
        FOREIGN KEY ([MembershipAssignmentId]) REFERENCES [dbo].[MembershipAssignments] ([Id]),

    CONSTRAINT [FK_Sales_CashTransactions]
        FOREIGN KEY ([CashTransactionId]) REFERENCES [dbo].[CashTransactions] ([Id]),

    -- ── CHECK constraints ─────────────────────────────────────────────────
    CONSTRAINT [CK_Sales_Status]         CHECK ([Status]        IN (0, 1, 2)),
    CONSTRAINT [CK_Sales_PaymentMethod]  CHECK ([PaymentMethod] IN (0, 1, 2)),
    CONSTRAINT [CK_Sales_Subtotal]       CHECK ([SubtotalAmount] >= 0),
    CONSTRAINT [CK_Sales_Discount]       CHECK ([DiscountAmount] >= 0),
    CONSTRAINT [CK_Sales_Total]          CHECK ([TotalAmount]    >= 0),
    CONSTRAINT [CK_Sales_TotalCalc]      CHECK ([TotalAmount] = [SubtotalAmount] - [DiscountAmount]),

    -- Anulación consistente
    CONSTRAINT [CK_Sales_Voided]
        CHECK (
            ([Status] <> 2 AND [VoidedAtUtc] IS NULL AND [VoidReason] IS NULL) OR
            ([Status]  = 2 AND [VoidedAtUtc] IS NOT NULL AND [VoidReason] IS NOT NULL)
        )
);
GO

CREATE NONCLUSTERED INDEX [IX_Sales_TenantId_Date]
    ON [dbo].[Sales] ([TenantId] ASC, [Date] DESC)
    WHERE [IsDeleted] = 0;
GO

CREATE NONCLUSTERED INDEX [IX_Sales_MemberId]
    ON [dbo].[Sales] ([MemberId] ASC)
    WHERE [IsDeleted] = 0;
GO

CREATE NONCLUSTERED INDEX [IX_Sales_TenantId_Status]
    ON [dbo].[Sales] ([TenantId] ASC, [Status] ASC, [Date] DESC)
    WHERE [IsDeleted] = 0;
GO

PRINT 'Tabla [dbo].[Sales] creada.';
GO

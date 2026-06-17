-- =============================================================================
--  GymGo  ·  01_schema / 14_Cash.sql
--  Transacciones de caja: ingresos y egresos del gimnasio.
--
--  Type (enum, int):
--      0 = Ingreso   → Dinero que entra al gimnasio
--      1 = Egreso    → Dinero que sale del gimnasio
--
--  PaymentMethod (enum, int):
--      0 = Efectivo
--      1 = Tarjeta
--      2 = Transferencia
--
--  Concept (enum, int):
--      -- Ingresos --
--      0  = CuotaMembresia    → Pago de cuota mensual
--      1  = Matricula         → Pago de matrícula / inscripción
--      2  = ProductoServicio  → Venta de producto o servicio extra
--      3  = OtroIngreso       → Ingreso sin categoría específica
--      -- Egresos --
--      10 = Servicios         → Luz, agua, gas, internet, etc.
--      11 = Mantencion        → Técnicos, reparaciones, equipamiento
--      12 = Insumos           → Materiales, artículos de limpieza, etc.
--      13 = OtroEgreso        → Gasto sin categoría específica
--
--  Anulación:
--      Las transacciones no se eliminan físicamente — se anulan (IsVoided = 1)
--      con fecha y motivo. Permanecen en el historial.
-- =============================================================================

USE [GymGoDb_Dev];
GO

IF OBJECT_ID(N'[dbo].[CashTransactions]', N'U') IS NOT NULL
BEGIN
    PRINT 'La tabla [dbo].[CashTransactions] ya existe. Sin cambios.';
    RETURN;
END
GO

CREATE TABLE [dbo].[CashTransactions]
(
    -- ── Identidad ────────────────────────────────────────────────────────
    [Id]                      UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_CashTransactions] PRIMARY KEY,
    [TenantId]                UNIQUEIDENTIFIER NOT NULL,

    -- ── Clasificación ─────────────────────────────────────────────────────
    [Date]                    DATE             NOT NULL,
    [Type]                    TINYINT          NOT NULL,   -- 0=Ingreso, 1=Egreso
    [Amount]                  DECIMAL(18,2)    NOT NULL,
    [PaymentMethod]           TINYINT          NOT NULL,   -- 0=Efectivo, 1=Tarjeta, 2=Transferencia
    [Concept]                 TINYINT          NOT NULL,   -- ver tabla arriba

    -- ── Descripción ───────────────────────────────────────────────────────
    -- Obligatoria en egresos para identificar el gasto (ej: "Factura Enel Junio")
    [Description]             NVARCHAR(500)    NULL,

    -- ── Vínculos opcionales (solo Ingresos) ──────────────────────────────
    [MemberId]                UNIQUEIDENTIFIER NULL,
    [MembershipAssignmentId]  UNIQUEIDENTIFIER NULL,

    -- ── Auditoría de quién cobró/pagó ─────────────────────────────────────
    [ProcessedByUserId]       UNIQUEIDENTIFIER NOT NULL,
    [CreatedAtUtc]            DATETIME2(3)     NOT NULL,

    -- ── Anulación ─────────────────────────────────────────────────────────
    [IsVoided]                BIT              NOT NULL CONSTRAINT [DF_CT_IsVoided]  DEFAULT (0),
    [VoidedAtUtc]             DATETIME2(3)     NULL,
    [VoidReason]              NVARCHAR(500)    NULL,

    -- ── FK físicas ────────────────────────────────────────────────────────
    CONSTRAINT [FK_CashTransactions_Tenants]
        FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants] ([Id]),

    CONSTRAINT [FK_CashTransactions_Members]
        FOREIGN KEY ([MemberId]) REFERENCES [dbo].[Members] ([Id]),

    CONSTRAINT [FK_CashTransactions_MembershipAssignments]
        FOREIGN KEY ([MembershipAssignmentId]) REFERENCES [dbo].[MembershipAssignments] ([Id]),

    CONSTRAINT [FK_CashTransactions_Users]
        FOREIGN KEY ([ProcessedByUserId]) REFERENCES [dbo].[Users] ([Id]),

    -- ── CHECK constraints ─────────────────────────────────────────────────
    CONSTRAINT [CK_CT_Type]           CHECK ([Type]          IN (0, 1)),
    CONSTRAINT [CK_CT_PaymentMethod]  CHECK ([PaymentMethod] IN (0, 1, 2)),
    CONSTRAINT [CK_CT_Concept]        CHECK ([Concept]       IN (0, 1, 2, 3, 10, 11, 12, 13)),
    CONSTRAINT [CK_CT_Amount]         CHECK ([Amount]        >  0),

    -- Egresos deben tener descripción
    CONSTRAINT [CK_CT_Egreso_Description]
        CHECK ([Type] = 0 OR [Description] IS NOT NULL),

    -- Socios solo aplican a ingresos
    CONSTRAINT [CK_CT_Member_Ingreso]
        CHECK ([Type] = 0 OR ([MemberId] IS NULL AND [MembershipAssignmentId] IS NULL)),

    -- Concepto coherente con el tipo
    --   Ingresos: 0-3  |  Egresos: 10-13
    CONSTRAINT [CK_CT_Concept_Type]
        CHECK (
            ([Type] = 0 AND [Concept] BETWEEN 0 AND 3)  OR
            ([Type] = 1 AND [Concept] BETWEEN 10 AND 13)
        ),

    -- Anulación consistente: si IsVoided=1 entonces ambos campos deben tener valor
    CONSTRAINT [CK_CT_Voided_Consistency]
        CHECK (
            ([IsVoided] = 0 AND [VoidedAtUtc] IS NULL AND [VoidReason] IS NULL) OR
            ([IsVoided] = 1 AND [VoidedAtUtc] IS NOT NULL AND [VoidReason] IS NOT NULL)
        )
);
GO

-- Índice principal: todas las queries van por tenant + fecha
CREATE NONCLUSTERED INDEX [IX_CashTransactions_TenantId_Date]
    ON [dbo].[CashTransactions] ([TenantId] ASC, [Date] DESC)
    INCLUDE ([Type], [Amount], [IsVoided]);
GO

-- Historial por socio
CREATE NONCLUSTERED INDEX [IX_CashTransactions_MemberId]
    ON [dbo].[CashTransactions] ([MemberId] ASC)
    WHERE [MemberId] IS NOT NULL;
GO

-- Filtro por tipo (para resumen de ingresos vs egresos)
CREATE NONCLUSTERED INDEX [IX_CashTransactions_TenantId_Type]
    ON [dbo].[CashTransactions] ([TenantId] ASC, [Type] ASC, [Date] DESC)
    WHERE [IsVoided] = 0;
GO

PRINT 'Tabla [dbo].[CashTransactions] creada.';
GO

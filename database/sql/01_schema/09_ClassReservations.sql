-- =============================================================================
--  GymGo  ·  01_schema / 09_ClassReservations.sql
--  Reservas de socios para sesiones concretas de clases programadas.
--
--  Una "sesión" queda identificada por (ClassScheduleId + SessionDate):
--  el horario semanal recurrente más la fecha exacta en que ocurrirá.
--
--  ReservationStatus (enum, int):
--      0 = Active            → Reserva vigente, el socio tiene lugar confirmado
--      1 = CancelledByMember → El socio anuló su propia reserva
--      2 = CancelledByStaff  → El staff anuló la reserva en nombre del socio
--      3 = NoShow            → El socio no se presentó a la sesión
--
--  Notas de diseño:
--    - No tiene soft delete: las reservas son registros de auditoría inmutables.
--    - MemberFullName es un snapshot del nombre al momento de la reserva.
--    - La unicidad de reservas activas (un socio, un horario, una fecha) se
--      garantiza en la capa Application antes de persistir.
-- =============================================================================

USE [GymGoDb_Dev];
GO

IF OBJECT_ID(N'[dbo].[ClassReservations]', N'U') IS NOT NULL
BEGIN
    PRINT 'La tabla [dbo].[ClassReservations] ya existe. Sin cambios.';
    RETURN;
END
GO

CREATE TABLE [dbo].[ClassReservations]
(
    -- ── Identidad ────────────────────────────────────────────────────────
    [Id]               UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_ClassReservations] PRIMARY KEY,
    [TenantId]         UNIQUEIDENTIFIER NOT NULL,

    -- ── Relaciones ───────────────────────────────────────────────────────
    [MemberId]         UNIQUEIDENTIFIER NOT NULL,
    [ClassScheduleId]  UNIQUEIDENTIFIER NOT NULL,

    -- ── Datos de la sesión ────────────────────────────────────────────────
    [SessionDate]      DATE             NOT NULL,
    [ReservedAtUtc]    DATETIME2(3)     NOT NULL,

    -- Snapshot del nombre del socio al momento de la reserva
    [MemberFullName]   NVARCHAR(200)    NOT NULL,
    [Notes]            NVARCHAR(500)    NULL,

    -- ── Estado ───────────────────────────────────────────────────────────
    [Status]           INT              NOT NULL CONSTRAINT [DF_CR_Status] DEFAULT (0),
    [CancelledAtUtc]   DATETIME2(3)     NULL,
    [CancelledBy]      NVARCHAR(200)    NULL,
    [CancelReason]     NVARCHAR(500)    NULL,

    -- ── IAuditable ───────────────────────────────────────────────────────
    [CreatedAtUtc]     DATETIME2(3)     NOT NULL,
    [CreatedBy]        NVARCHAR(200)    NULL,
    [ModifiedAtUtc]    DATETIME2(3)     NULL,
    [ModifiedBy]       NVARCHAR(200)    NULL,

    -- ── FK físicas ────────────────────────────────────────────────────────
    CONSTRAINT [FK_ClassReservations_Members]
        FOREIGN KEY ([MemberId]) REFERENCES [dbo].[Members] ([Id]),

    CONSTRAINT [FK_ClassReservations_ClassSchedules]
        FOREIGN KEY ([ClassScheduleId]) REFERENCES [dbo].[ClassSchedules] ([Id]),

    CONSTRAINT [FK_ClassReservations_Tenants]
        FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants] ([Id]),

    -- ── CHECK constraints ─────────────────────────────────────────────────
    CONSTRAINT [CK_CR_Status] CHECK ([Status] BETWEEN 0 AND 3)
);
GO

-- Reservas activas de una sesión (consulta de capacidad y lista de asistentes)
CREATE NONCLUSTERED INDEX [IX_ClassReservations_Schedule_Date]
    ON [dbo].[ClassReservations] ([ClassScheduleId] ASC, [SessionDate] ASC)
    INCLUDE ([Status], [MemberId], [MemberFullName]);
GO

-- Historial de reservas de un socio (consulta desde perfil del socio)
CREATE NONCLUSTERED INDEX [IX_ClassReservations_Member]
    ON [dbo].[ClassReservations] ([TenantId] ASC, [MemberId] ASC, [SessionDate] DESC)
    INCLUDE ([Status], [ClassScheduleId]);
GO

PRINT 'Tabla [dbo].[ClassReservations] creada.';
GO

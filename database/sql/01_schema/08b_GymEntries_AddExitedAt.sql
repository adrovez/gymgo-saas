-- =============================================================================
--  GymGo  ·  01_schema / 08b_GymEntries_AddExitedAt.sql
--  Agrega la columna ExitedAtUtc a la tabla GymEntries para registrar
--  la hora de salida del socio.
--
--  ExitedAtUtc es NULL mientras el socio no ha registrado su salida.
--  Al registrar la salida se establece con el timestamp UTC actual.
-- =============================================================================

USE [GymGoDb_Dev];
GO

IF COL_LENGTH(N'dbo.GymEntries', N'ExitedAtUtc') IS NOT NULL
BEGIN
    PRINT 'La columna [ExitedAtUtc] ya existe en [dbo].[GymEntries]. Sin cambios.';
    RETURN;
END
GO

ALTER TABLE [dbo].[GymEntries]
    ADD [ExitedAtUtc] DATETIME2(3) NULL;
GO

PRINT 'Columna [ExitedAtUtc] agregada a [dbo].[GymEntries].';
GO

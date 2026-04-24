-- =============================================================================
--  GymGo  ·  00_database / 01_CreateDatabase.sql
--  Crea la base de datos GymGoDb_Dev en SQL Server LocalDB.
--  Idempotente: no falla si ya existe.
-- =============================================================================

IF DB_ID(N'GymGoDb_Dev') IS NULL
BEGIN
    PRINT 'Creando base de datos [GymGoDb_Dev]...';
    CREATE DATABASE [GymGoDb_Dev];
END
ELSE
BEGIN
    PRINT 'La base de datos [GymGoDb_Dev] ya existe. Sin cambios.';
END
GO

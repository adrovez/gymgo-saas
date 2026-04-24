-- =============================================================================
--  GymGo  ·  01_schema / 01_Tenants.sql
--  Tabla raíz del aislamiento multi-tenant. Cada Tenant = un gimnasio cliente.
-- =============================================================================

USE [GymGoDb_Dev];
GO

IF OBJECT_ID(N'[dbo].[Tenants]', N'U') IS NOT NULL
BEGIN
    PRINT 'La tabla [dbo].[Tenants] ya existe. Sin cambios.';
    RETURN;
END
GO

CREATE TABLE [dbo].[Tenants]
(
    [Id]              UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_Tenants] PRIMARY KEY,
    [Name]            NVARCHAR(150)    NOT NULL,
    [Slug]            NVARCHAR(60)     NOT NULL,
    [ContactEmail]    NVARCHAR(200)    NULL,
    [ContactPhone]    NVARCHAR(40)     NULL,
    [IsActive]        BIT              NOT NULL CONSTRAINT [DF_Tenants_IsActive] DEFAULT (1),

    -- IAuditable
    [CreatedAtUtc]    DATETIME2(3)     NOT NULL,
    [CreatedBy]       NVARCHAR(200)    NULL,
    [ModifiedAtUtc]   DATETIME2(3)     NULL,
    [ModifiedBy]      NVARCHAR(200)    NULL
);
GO

-- El slug identifica al tenant en URLs / headers; debe ser único.
CREATE UNIQUE NONCLUSTERED INDEX [UX_Tenants_Slug]
    ON [dbo].[Tenants] ([Slug] ASC);
GO

PRINT 'Tabla [dbo].[Tenants] creada.';
GO

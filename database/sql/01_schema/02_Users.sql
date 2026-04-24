-- =============================================================================
--  GymGo  ·  01_schema / 02_Users.sql
--  Usuarios del sistema. PlatformAdmin (Role=0) puede tener TenantId vacío.
--  El resto pertenece a un único tenant.
--
--  Roles (UserRole enum, persistido como int):
--      0 = PlatformAdmin
--      1 = GymOwner
--      2 = GymStaff
--      3 = Instructor
--      4 = Member
-- =============================================================================

USE [GymGoDb_Dev];
GO

IF OBJECT_ID(N'[dbo].[Users]', N'U') IS NOT NULL
BEGIN
    PRINT 'La tabla [dbo].[Users] ya existe. Sin cambios.';
    RETURN;
END
GO

CREATE TABLE [dbo].[Users]
(
    [Id]              UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_Users] PRIMARY KEY,
    [TenantId]        UNIQUEIDENTIFIER NOT NULL,
    [Email]           NVARCHAR(200)    NOT NULL,
    [PasswordHash]    NVARCHAR(300)    NOT NULL,
    [FullName]        NVARCHAR(200)    NOT NULL,
    [Role]            INT              NOT NULL,
    [IsActive]        BIT              NOT NULL CONSTRAINT [DF_Users_IsActive] DEFAULT (1),
    [LastLoginUtc]    DATETIME2(3)     NULL,

    -- IAuditable
    [CreatedAtUtc]    DATETIME2(3)     NOT NULL,
    [CreatedBy]       NVARCHAR(200)    NULL,
    [ModifiedAtUtc]   DATETIME2(3)     NULL,
    [ModifiedBy]      NVARCHAR(200)    NULL,

    -- ISoftDeletable
    [IsDeleted]       BIT              NOT NULL CONSTRAINT [DF_Users_IsDeleted] DEFAULT (0),
    [DeletedAtUtc]    DATETIME2(3)     NULL,
    [DeletedBy]       NVARCHAR(200)    NULL,

    CONSTRAINT [CK_Users_Role] CHECK ([Role] BETWEEN 0 AND 4)
);
GO

-- Email único por tenant (un mismo email puede existir en distintos gimnasios).
CREATE UNIQUE NONCLUSTERED INDEX [UX_Users_TenantId_Email]
    ON [dbo].[Users] ([TenantId] ASC, [Email] ASC)
    WHERE [IsDeleted] = 0;
GO

-- Index secundario por TenantId para queries filtradas por tenant.
CREATE NONCLUSTERED INDEX [IX_Users_TenantId]
    ON [dbo].[Users] ([TenantId] ASC);
GO

PRINT 'Tabla [dbo].[Users] creada.';
GO

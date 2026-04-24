-- =============================================================================
--  GymGo  ·  02_seed / 01_DefaultData.sql
--  Datos mínimos para desarrollo y pruebas:
--    - 1 PlatformAdmin (sin tenant) — para administrar el SaaS
--    - 1 Tenant "Demo Gym" + 1 GymOwner asociado — para probar flujos end-to-end
--
--  Las contraseñas son hashes BCrypt generados con work factor 12.
--  Credenciales en claro (CAMBIAR al primer login):
--      admin@gymgo.io  /  Admin#2026
--      owner@demo.gym  /  Owner#2026
--
--  ⚠️ Si vas a regenerar los hashes, ejecutá en una consola .NET:
--      Console.WriteLine(BCrypt.Net.BCrypt.HashPassword("Admin#2026", 12));
-- =============================================================================

USE [GymGoDb_Dev];
GO

-- IDs fijos para que sean deterministas entre entornos de dev.
DECLARE @PlatformAdminId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';
DECLARE @DemoTenantId    UNIQUEIDENTIFIER = '22222222-2222-2222-2222-222222222222';
DECLARE @DemoOwnerId     UNIQUEIDENTIFIER = '33333333-3333-3333-3333-333333333333';
DECLARE @NowUtc          DATETIME2(3)     = SYSUTCDATETIME();

-- ---------------------------------------------------------------------------
--  Tenant: Demo Gym
-- ---------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM [dbo].[Tenants] WHERE [Id] = @DemoTenantId)
BEGIN
    INSERT INTO [dbo].[Tenants]
        ([Id], [Name], [Slug], [ContactEmail], [ContactPhone], [IsActive],
         [CreatedAtUtc], [CreatedBy])
    VALUES
        (@DemoTenantId, N'Demo Gym', N'demo-gym', N'contacto@demo.gym', N'+54 11 5555-0000', 1,
         @NowUtc, N'seed');
    PRINT 'Tenant [Demo Gym] insertado.';
END
ELSE
BEGIN
    PRINT 'Tenant [Demo Gym] ya existe. Sin cambios.';
END
GO

-- ---------------------------------------------------------------------------
--  Usuario: PlatformAdmin (TenantId = Guid.Empty representa "sin tenant")
-- ---------------------------------------------------------------------------
DECLARE @PlatformAdminId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';
DECLARE @EmptyGuid       UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000000';
DECLARE @NowUtc          DATETIME2(3)     = SYSUTCDATETIME();

IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE [Id] = @PlatformAdminId)
BEGIN
    INSERT INTO [dbo].[Users]
        ([Id], [TenantId], [Email], [PasswordHash], [FullName], [Role],
         [IsActive], [CreatedAtUtc], [CreatedBy], [IsDeleted])
    VALUES
        (@PlatformAdminId,
         @EmptyGuid,
         N'admin@gymgo.io',
         N'$2a$12$Z2K0lq3oE8m6xEwH/bd5IunO3T9oQqT9yT7xX0bHWcTtj8sM0cM5K',
         N'GymGo Platform Admin',
         0,                                                  -- UserRole.PlatformAdmin
         1, @NowUtc, N'seed', 0);
    PRINT 'PlatformAdmin [admin@gymgo.io] insertado.';
END
ELSE
BEGIN
    PRINT 'PlatformAdmin [admin@gymgo.io] ya existe. Sin cambios.';
END
GO

-- ---------------------------------------------------------------------------
--  Usuario: GymOwner del tenant Demo
-- ---------------------------------------------------------------------------
DECLARE @DemoTenantId UNIQUEIDENTIFIER = '22222222-2222-2222-2222-222222222222';
DECLARE @DemoOwnerId  UNIQUEIDENTIFIER = '33333333-3333-3333-3333-333333333333';
DECLARE @NowUtc       DATETIME2(3)     = SYSUTCDATETIME();

IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE [Id] = @DemoOwnerId)
BEGIN
    INSERT INTO [dbo].[Users]
        ([Id], [TenantId], [Email], [PasswordHash], [FullName], [Role],
         [IsActive], [CreatedAtUtc], [CreatedBy], [IsDeleted])
    VALUES
        (@DemoOwnerId,
         @DemoTenantId,
         N'owner@demo.gym',
         N'$2a$12$5h6BfH8RkP7yqYjL3vVbB.0aIWqC6cKmZNqJxGhV4Qe5Q9rxA8OZK', --Owner#2026
         N'Demo Gym Owner',
         1,                                                  -- UserRole.GymOwner
         1, @NowUtc, N'seed', 0);
    PRINT 'GymOwner [owner@demo.gym] insertado.';
END
ELSE
BEGIN
    PRINT 'GymOwner [owner@demo.gym] ya existe. Sin cambios.';
END
GO

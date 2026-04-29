using FluentAssertions;
using GymGo.Application.Auth.Commands.Login;
using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using GymGo.Domain.GymClasses;
using GymGo.Domain.Members;
using GymGo.Domain.MembershipAssignments;
using GymGo.Domain.MembershipPlans;
using GymGo.Domain.Tenants;
using GymGo.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace GymGo.UnitTests.Application.Auth;

/// <summary>
/// Tests unitarios para <see cref="LoginCommandHandler"/>.
/// Usa Moq para IPasswordHasher / IJwtTokenGenerator y un
/// DbContext InMemory para simular la capa de datos.
/// </summary>
public sealed class LoginCommandHandlerTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static User BuildUser(
        Guid tenantId,
        string email       = "admin@gym.com",
        string passwordHash = "HASH_OK",
        UserRole role       = UserRole.GymOwner,
        bool isActive       = true)
    {
        var user = User.Create(tenantId, email, passwordHash, "Nombre Apellido", role);
        if (!isActive) user.Deactivate();
        return user;
    }

    private static AuthTestDbContext BuildDb(User? user = null, Tenant? tenant = null)
    {
        var options = new DbContextOptionsBuilder<AuthTestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var db = new AuthTestDbContext(options);
        if (user   is not null) db.Set<User>().Add(user);
        if (tenant is not null) db.Set<Tenant>().Add(tenant);
        db.SaveChanges();
        return db;
    }

    private static LoginCommandHandler BuildHandler(
        AuthTestDbContext db,
        Mock<IPasswordHasher>    hasher,
        Mock<IJwtTokenGenerator> jwtGen)
        => new(db, hasher.Object, jwtGen.Object);

    // ── Casos felices ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_credenciales_validas_retorna_token()
    {
        var tenant = Tenant.Create("GymTest", "gymtest");
        var user   = BuildUser(tenant.Id);
        var db     = BuildDb(user, tenant);

        var hasher = new Mock<IPasswordHasher>();
        hasher.Setup(h => h.Verify("pass123", "HASH_OK")).Returns(true);

        var expiresAt = DateTime.UtcNow.AddHours(1);
        var jwtGen = new Mock<IJwtTokenGenerator>();
        jwtGen.Setup(g => g.Generate(It.IsAny<User>(), out expiresAt))
              .Returns("JWT_TOKEN");

        var handler = BuildHandler(db, hasher, jwtGen);
        var result  = await handler.Handle(new LoginCommand("admin@gym.com", "pass123"), default);

        result.Token.Should().Be("JWT_TOKEN");
        result.Email.Should().Be("admin@gym.com");
        result.Role.Should().Be(UserRole.GymOwner);
        result.TenantId.Should().Be(tenant.Id);
    }

    // ── Credenciales inválidas ────────────────────────────────────────────────

    [Fact]
    public async Task Handle_email_inexistente_lanza_AUTH_INVALID_CREDENTIALS()
    {
        var db     = BuildDb(); // sin usuarios
        var hasher = new Mock<IPasswordHasher>();
        var jwtGen = new Mock<IJwtTokenGenerator>();

        var handler = BuildHandler(db, hasher, jwtGen);
        var act     = () => handler.Handle(new LoginCommand("noexiste@gym.com", "pass"), default);

        await act.Should().ThrowAsync<BusinessRuleViolationException>()
            .Where(e => e.Code == "AUTH_INVALID_CREDENTIALS");
    }

    [Fact]
    public async Task Handle_password_incorrecto_lanza_AUTH_INVALID_CREDENTIALS()
    {
        var tenant = Tenant.Create("GymTest", "gymtest");
        var user   = BuildUser(tenant.Id);
        var db     = BuildDb(user, tenant);

        var hasher = new Mock<IPasswordHasher>();
        hasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        var jwtGen  = new Mock<IJwtTokenGenerator>();
        var handler = BuildHandler(db, hasher, jwtGen);

        var act = () => handler.Handle(new LoginCommand("admin@gym.com", "wrong"), default);

        await act.Should().ThrowAsync<BusinessRuleViolationException>()
            .Where(e => e.Code == "AUTH_INVALID_CREDENTIALS");
    }

    // ── Estado de cuenta ──────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_usuario_inactivo_lanza_AUTH_USER_INACTIVE()
    {
        var tenant = Tenant.Create("GymTest", "gymtest");
        var user   = BuildUser(tenant.Id, isActive: false);
        var db     = BuildDb(user, tenant);

        var hasher = new Mock<IPasswordHasher>();
        hasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        var jwtGen  = new Mock<IJwtTokenGenerator>();
        var handler = BuildHandler(db, hasher, jwtGen);

        var act = () => handler.Handle(new LoginCommand("admin@gym.com", "pass"), default);

        await act.Should().ThrowAsync<BusinessRuleViolationException>()
            .Where(e => e.Code == "AUTH_USER_INACTIVE");
    }

    [Fact]
    public async Task Handle_tenant_inactivo_lanza_AUTH_TENANT_INACTIVE()
    {
        var tenant = Tenant.Create("GymTest", "gymtest");
        tenant.Deactivate();

        var user   = BuildUser(tenant.Id);
        var db     = BuildDb(user, tenant);

        var hasher = new Mock<IPasswordHasher>();
        hasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        var jwtGen  = new Mock<IJwtTokenGenerator>();
        var handler = BuildHandler(db, hasher, jwtGen);

        var act = () => handler.Handle(new LoginCommand("admin@gym.com", "pass"), default);

        await act.Should().ThrowAsync<BusinessRuleViolationException>()
            .Where(e => e.Code == "AUTH_TENANT_INACTIVE");
    }

    // ── PlatformAdmin (sin tenant) ────────────────────────────────────────────

    [Fact]
    public async Task Handle_PlatformAdmin_sin_tenant_retorna_token()
    {
        // PlatformAdmin: TenantId = Guid.Empty, no necesita Tenant en la BD
        var user = User.Create(Guid.Empty, "platform@gymgo.com", "HASH_OK", "Admin GymGo", UserRole.PlatformAdmin);
        var db   = BuildDb(user); // sin tenant

        var hasher = new Mock<IPasswordHasher>();
        hasher.Setup(h => h.Verify("pass", "HASH_OK")).Returns(true);

        var expiresAt = DateTime.UtcNow.AddHours(1);
        var jwtGen    = new Mock<IJwtTokenGenerator>();
        jwtGen.Setup(g => g.Generate(It.IsAny<User>(), out expiresAt))
              .Returns("JWT_PLATFORM");

        var handler = BuildHandler(db, hasher, jwtGen);
        var result  = await handler.Handle(new LoginCommand("platform@gymgo.com", "pass"), default);

        result.Token.Should().Be("JWT_PLATFORM");
        result.Role.Should().Be(UserRole.PlatformAdmin);
        result.TenantId.Should().Be(Guid.Empty);
    }
}

// ── DbContext InMemory mínimo ────────────────────────────────────────────────

/// <summary>
/// DbContext InMemory para los tests de Auth. Sin query filters para
/// no depender de ICurrentTenant en los tests unitarios del handler.
/// </summary>
internal sealed class AuthTestDbContext : DbContext, IApplicationDbContext
{
    public AuthTestDbContext(DbContextOptions<AuthTestDbContext> options) : base(options) { }

    public DbSet<User>                   Users                 => Set<User>();
    public DbSet<Tenant>                 Tenants               => Set<Tenant>();
    public DbSet<Member>                 Members               => Set<Member>();
    public DbSet<MembershipPlan>         MembershipPlans       => Set<MembershipPlan>();
    public DbSet<MembershipAssignment>   MembershipAssignments => Set<MembershipAssignment>();
    public DbSet<GymClass>               GymClasses            => Set<GymClass>();
    public DbSet<ClassSchedule>          ClassSchedules        => Set<ClassSchedule>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<User>().HasKey(u => u.Id);
        mb.Entity<User>().Property(u => u.TenantId);
        mb.Entity<User>().Property(u => u.Email);
        mb.Entity<User>().Property(u => u.PasswordHash);
        mb.Entity<User>().Property(u => u.FullName);
        mb.Entity<User>().Property(u => u.IsActive);
        mb.Entity<User>().Property(u => u.IsDeleted);

        mb.Entity<Tenant>().HasKey(t => t.Id);
        mb.Entity<Tenant>().Property(t => t.IsActive);

        mb.Entity<Member>().HasKey(m => m.Id);
        mb.Entity<MembershipPlan>().HasKey(p => p.Id);
        mb.Entity<MembershipAssignment>().HasKey(a => a.Id);

        mb.Entity<GymClass>().HasKey(c => c.Id);
        mb.Entity<GymClass>().Ignore(c => c.DomainEvents);
        mb.Entity<ClassSchedule>().HasKey(s => s.Id);
        mb.Entity<ClassSchedule>().Ignore(s => s.DomainEvents);
        mb.Entity<ClassSchedule>().Property(s => s.DayOfWeek).HasConversion<int>();
        mb.Entity<GymClass>()
            .HasMany(c => c.Schedules)
            .WithOne(s => s.GymClass)
            .HasForeignKey(s => s.GymClassId);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => base.SaveChangesAsync(cancellationToken);
}

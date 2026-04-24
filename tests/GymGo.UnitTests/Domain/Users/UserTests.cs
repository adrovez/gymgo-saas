using FluentAssertions;
using GymGo.Domain.Exceptions;
using GymGo.Domain.Users;

namespace GymGo.UnitTests.Domain.Users;

public class UserTests
{
    private static readonly Guid TenantId = Guid.NewGuid();

    [Fact]
    public void Create_con_valores_validos_devuelve_usuario_activo()
    {
        var user = User.Create(TenantId, "Owner@Demo.Gym", "hashedPwd", "  Demo Owner  ", UserRole.GymOwner);

        user.Id.Should().NotBe(Guid.Empty);
        user.TenantId.Should().Be(TenantId);
        user.Email.Should().Be("owner@demo.gym");           // normalizado
        user.FullName.Should().Be("Demo Owner");             // trim
        user.Role.Should().Be(UserRole.GymOwner);
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_PlatformAdmin_no_requiere_tenant()
    {
        var user = User.Create(Guid.Empty, "admin@gymgo.io", "hash", "Admin", UserRole.PlatformAdmin);
        user.Role.Should().Be(UserRole.PlatformAdmin);
        user.TenantId.Should().Be(Guid.Empty);
    }

    [Fact]
    public void Create_no_PlatformAdmin_sin_tenant_lanza_excepcion()
    {
        var act = () => User.Create(Guid.Empty, "u@x.io", "hash", "Name", UserRole.GymOwner);
        act.Should().Throw<BusinessRuleViolationException>()
           .Where(e => e.Code == "USER_TENANT_REQUIRED");
    }

    [Theory]
    [InlineData("sin-arroba")]
    [InlineData("")]
    public void Create_con_email_invalido_lanza_excepcion(string email)
    {
        var act = () => User.Create(TenantId, email, "hash", "Name", UserRole.Member);
        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void RegisterLogin_actualiza_LastLoginUtc()
    {
        var user = User.Create(TenantId, "u@x.io", "hash", "Name", UserRole.Member);
        user.LastLoginUtc.Should().BeNull();

        user.RegisterLogin();

        user.LastLoginUtc.Should().NotBeNull();
        user.LastLoginUtc!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }
}

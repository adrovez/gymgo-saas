using FluentAssertions;
using GymGo.Domain.Exceptions;
using GymGo.Domain.Tenants;

namespace GymGo.UnitTests.Domain.Tenants;

public class TenantTests
{
    [Fact]
    public void Create_con_valores_validos_devuelve_tenant_activo()
    {
        // Arrange & Act
        var tenant = Tenant.Create("Demo Gym", "demo-gym", "contacto@demo.gym", "+5491155550000");

        // Assert
        tenant.Id.Should().NotBe(Guid.Empty);
        tenant.Name.Should().Be("Demo Gym");
        tenant.Slug.Should().Be("demo-gym");
        tenant.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_normaliza_slug_a_minusculas_y_recorta_espacios()
    {
        var tenant = Tenant.Create("  Demo  ", "  Demo-Gym  ");
        tenant.Slug.Should().Be("demo-gym");
        tenant.Name.Should().Be("Demo");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_sin_nombre_lanza_excepcion(string? name)
    {
        var act = () => Tenant.Create(name!, "demo");
        act.Should().Throw<BusinessRuleViolationException>()
           .Where(e => e.Code == "TENANT_NAME_REQUIRED");
    }

    [Fact]
    public void Create_con_slug_demasiado_largo_lanza_excepcion()
    {
        var slug = new string('a', 61);
        var act = () => Tenant.Create("Demo", slug);
        act.Should().Throw<BusinessRuleViolationException>()
           .Where(e => e.Code == "TENANT_SLUG_TOO_LONG");
    }

    [Fact]
    public void Deactivate_marca_el_tenant_como_inactivo()
    {
        var tenant = Tenant.Create("Demo", "demo");
        tenant.Deactivate();
        tenant.IsActive.Should().BeFalse();
    }
}

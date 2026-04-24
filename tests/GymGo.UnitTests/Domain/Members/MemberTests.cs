using FluentAssertions;
using GymGo.Domain.Exceptions;
using GymGo.Domain.Members;

namespace GymGo.UnitTests.Domain.Members;

public class MemberTests
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly DateOnly ValidBirthDate = new(1990, 5, 15);

    // ── Helpers ───────────────────────────────────────────────────────────

    private static Member CreateValid(
        string rut = "12345678-9",
        string firstName = "Juan",
        string lastName = "Pérez",
        DateOnly? birthDate = null) =>
        Member.Create(
            tenantId: TenantId,
            rut: rut,
            firstName: firstName,
            lastName: lastName,
            birthDate: birthDate ?? ValidBirthDate);

    // ── Creación exitosa ──────────────────────────────────────────────────

    [Fact]
    public void Create_con_valores_minimos_validos_retorna_socio_activo()
    {
        var member = CreateValid();

        member.Id.Should().NotBe(Guid.Empty);
        member.TenantId.Should().Be(TenantId);
        member.FirstName.Should().Be("Juan");
        member.LastName.Should().Be("Pérez");
        member.Status.Should().Be(MemberStatus.Active);
        member.Gender.Should().Be(Gender.NotSpecified);
        member.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Create_normaliza_rut_quitando_puntos()
    {
        var member = Member.Create(TenantId, "12.345.678-9", "Ana", "García", ValidBirthDate);
        member.Rut.Should().Be("12345678-9");
    }

    [Fact]
    public void Create_normaliza_rut_sin_guion_inserta_guion()
    {
        var member = Member.Create(TenantId, "123456789", "Ana", "García", ValidBirthDate);
        member.Rut.Should().Be("12345678-9");
    }

    [Fact]
    public void Create_normaliza_email_a_minusculas_y_trim()
    {
        var member = Member.Create(TenantId, "12345678-9", "Ana", "García", ValidBirthDate,
            email: "  Juan.Perez@DEMO.cl  ");
        member.Email.Should().Be("juan.perez@demo.cl");
    }

    [Fact]
    public void Create_registration_date_es_hoy_si_no_se_especifica()
    {
        var member = CreateValid();
        member.RegistrationDate.Should().Be(DateOnly.FromDateTime(DateTime.UtcNow));
    }

    [Fact]
    public void Create_registration_date_respeta_fecha_custom()
    {
        var customDate = new DateOnly(2025, 1, 10);
        var member = Member.Create(TenantId, "12345678-9", "Ana", "García", ValidBirthDate,
            registrationDate: customDate);
        member.RegistrationDate.Should().Be(customDate);
    }

    [Fact]
    public void Create_con_todos_los_campos_opcionales()
    {
        var member = Member.Create(
            tenantId: TenantId,
            rut: "12345678-9",
            firstName: "Carlos",
            lastName: "González",
            birthDate: ValidBirthDate,
            gender: Gender.Male,
            email: "carlos@demo.cl",
            phone: "+56912345678",
            address: "Av. Siempre Viva 742",
            emergencyContactName: "María González",
            emergencyContactPhone: "+56987654321",
            registrationDate: new DateOnly(2026, 1, 1),
            notes: "Socio VIP");

        member.Gender.Should().Be(Gender.Male);
        member.Phone.Should().Be("+56912345678");
        member.Address.Should().Be("Av. Siempre Viva 742");
        member.EmergencyContactName.Should().Be("María González");
        member.Notes.Should().Be("Socio VIP");
    }

    // ── Validación de RUT ─────────────────────────────────────────────────

    [Theory]
    [InlineData("12345678-9")]     // RUT conocido válido
    [InlineData("12.345.678-9")]   // con puntos
    [InlineData("123456789")]      // sin guión
    [InlineData("11111111-1")]     // RUT válido conocido
    public void Create_acepta_rut_valido(string rut)
    {
        var act = () => Member.Create(TenantId, rut, "Ana", "García", ValidBirthDate);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("12345678-0")]     // dígito verificador incorrecto
    [InlineData("00000000-0")]     // RUT cero, inválido
    [InlineData("ABCDEFGH-K")]     // letras en el cuerpo
    [InlineData("1234-5")]         // demasiado corto y dígito incorrecto
    public void Create_rechaza_rut_invalido(string rut)
    {
        var act = () => Member.Create(TenantId, rut, "Ana", "García", ValidBirthDate);
        act.Should().Throw<BusinessRuleViolationException>()
           .Where(e => e.Code == "MEMBER_RUT_INVALID");
    }

    [Fact]
    public void Create_con_rut_vacio_lanza_MEMBER_RUT_REQUIRED()
    {
        var act = () => Member.Create(TenantId, "", "Ana", "García", ValidBirthDate);
        act.Should().Throw<BusinessRuleViolationException>()
           .Where(e => e.Code == "MEMBER_RUT_REQUIRED");
    }

    // ── Validación de campos obligatorios ─────────────────────────────────

    [Fact]
    public void Create_sin_tenant_lanza_MEMBER_TENANT_REQUIRED()
    {
        var act = () => Member.Create(Guid.Empty, "12345678-9", "Ana", "García", ValidBirthDate);
        act.Should().Throw<BusinessRuleViolationException>()
           .Where(e => e.Code == "MEMBER_TENANT_REQUIRED");
    }

    [Fact]
    public void Create_sin_nombre_lanza_MEMBER_FIRSTNAME_REQUIRED()
    {
        var act = () => Member.Create(TenantId, "12345678-9", "", "García", ValidBirthDate);
        act.Should().Throw<BusinessRuleViolationException>()
           .Where(e => e.Code == "MEMBER_FIRSTNAME_REQUIRED");
    }

    [Fact]
    public void Create_sin_apellido_lanza_MEMBER_LASTNAME_REQUIRED()
    {
        var act = () => Member.Create(TenantId, "12345678-9", "Ana", "", ValidBirthDate);
        act.Should().Throw<BusinessRuleViolationException>()
           .Where(e => e.Code == "MEMBER_LASTNAME_REQUIRED");
    }

    [Fact]
    public void Create_fecha_nacimiento_futura_lanza_MEMBER_BIRTHDATE_INVALID()
    {
        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var act = () => Member.Create(TenantId, "12345678-9", "Ana", "García", futureDate);
        act.Should().Throw<BusinessRuleViolationException>()
           .Where(e => e.Code == "MEMBER_BIRTHDATE_INVALID");
    }

    [Fact]
    public void Create_email_sin_arroba_lanza_MEMBER_EMAIL_INVALID()
    {
        var act = () => Member.Create(TenantId, "12345678-9", "Ana", "García", ValidBirthDate,
            email: "no-es-un-email");
        act.Should().Throw<BusinessRuleViolationException>()
           .Where(e => e.Code == "MEMBER_EMAIL_INVALID");
    }

    [Fact]
    public void Create_notas_mas_de_1000_chars_lanza_MEMBER_NOTES_TOO_LONG()
    {
        var notas = new string('x', 1001);
        var act = () => Member.Create(TenantId, "12345678-9", "Ana", "García", ValidBirthDate,
            notes: notas);
        act.Should().Throw<BusinessRuleViolationException>()
           .Where(e => e.Code == "MEMBER_NOTES_TOO_LONG");
    }

    // ── Cambio de estado ──────────────────────────────────────────────────

    [Fact]
    public void Suspend_cambia_status_a_Suspended()
    {
        var member = CreateValid();
        member.Suspend();
        member.Status.Should().Be(MemberStatus.Suspended);
    }

    [Fact]
    public void MarkAsDelinquent_cambia_status_a_Delinquent()
    {
        var member = CreateValid();
        member.MarkAsDelinquent();
        member.Status.Should().Be(MemberStatus.Delinquent);
    }

    [Fact]
    public void Activate_desde_Suspended_vuelve_a_Active()
    {
        var member = CreateValid();
        member.Suspend();
        member.Activate();
        member.Status.Should().Be(MemberStatus.Active);
    }

    [Fact]
    public void Activate_desde_Delinquent_vuelve_a_Active()
    {
        var member = CreateValid();
        member.MarkAsDelinquent();
        member.Activate();
        member.Status.Should().Be(MemberStatus.Active);
    }

    // ── Actualización ─────────────────────────────────────────────────────

    [Fact]
    public void Update_modifica_datos_de_contacto()
    {
        var member = CreateValid();
        member.Update("Pedro", "Soto", new DateOnly(1985, 3, 20),
            Gender.Male, "pedro@demo.cl", "+56911111111",
            "Calle Nueva 100", "Contacto", "+56922222222", "Nota nueva");

        member.FirstName.Should().Be("Pedro");
        member.LastName.Should().Be("Soto");
        member.Email.Should().Be("pedro@demo.cl");
        member.Gender.Should().Be(Gender.Male);
    }

    [Fact]
    public void Update_no_modifica_el_rut()
    {
        var member = CreateValid("12345678-9");
        var originalRut = member.Rut;

        member.Update("Pedro", "Soto", ValidBirthDate,
            Gender.NotSpecified, null, null, null, null, null, null);

        member.Rut.Should().Be(originalRut);
    }

    [Fact]
    public void Update_con_nombre_vacio_lanza_excepcion()
    {
        var member = CreateValid();
        var act = () => member.Update("", "Soto", ValidBirthDate,
            Gender.NotSpecified, null, null, null, null, null, null);
        act.Should().Throw<BusinessRuleViolationException>()
           .Where(e => e.Code == "MEMBER_FIRSTNAME_REQUIRED");
    }
}

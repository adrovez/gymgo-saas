using FluentAssertions;
using GymGo.Domain.Exceptions;
using GymGo.Domain.MembershipPlans;

namespace GymGo.UnitTests.Domain.MembershipPlans;

public class MembershipPlanTests
{
    private static readonly Guid TenantId = Guid.NewGuid();

    // ── Helpers ───────────────────────────────────────────────────────────

    /// <summary>Crea el plan más simple posible: mensual, todos los días, horario libre.</summary>
    private static MembershipPlan CreateFullFree(
        string name = "Plan Mensual Full",
        Periodicity periodicity = Periodicity.Monthly,
        decimal amount = 30000m) =>
        MembershipPlan.Create(
            tenantId: TenantId, name: name, description: null,
            periodicity: periodicity, daysPerWeek: 7, fixedDays: false,
            monday: false, tuesday: false, wednesday: false, thursday: false,
            friday: false, saturday: false, sunday: false,
            freeSchedule: true, timeFrom: null, timeTo: null,
            amount: amount);

    /// <summary>Plan con días y horario fijos: L-M-V de 08:00 a 10:00.</summary>
    private static MembershipPlan CreateFixedLMV() =>
        MembershipPlan.Create(
            tenantId: TenantId, name: "Plan L-M-V Mañana", description: null,
            periodicity: Periodicity.Monthly, daysPerWeek: 3, fixedDays: true,
            monday: true, tuesday: true, wednesday: false, thursday: false,
            friday: true, saturday: false, sunday: false,
            freeSchedule: false,
            timeFrom: new TimeOnly(8, 0), timeTo: new TimeOnly(10, 0),
            amount: 20000m);

    // ── Creación exitosa ──────────────────────────────────────────────────

    [Fact]
    public void Create_plan_libre_retorna_entidad_activa()
    {
        var plan = CreateFullFree();

        plan.Id.Should().NotBe(Guid.Empty);
        plan.TenantId.Should().Be(TenantId);
        plan.IsActive.Should().BeTrue();
        plan.IsDeleted.Should().BeFalse();
        plan.DaysPerWeek.Should().Be(7);
        plan.FixedDays.Should().BeFalse();
        plan.FreeSchedule.Should().BeTrue();
    }

    [Fact]
    public void Create_plan_fijo_LMV_horario_manana()
    {
        var plan = CreateFixedLMV();

        plan.FixedDays.Should().BeTrue();
        plan.DaysPerWeek.Should().Be(3);
        plan.Monday.Should().BeTrue();
        plan.Tuesday.Should().BeTrue();
        plan.Wednesday.Should().BeFalse();
        plan.Thursday.Should().BeFalse();
        plan.Friday.Should().BeTrue();
        plan.FreeSchedule.Should().BeFalse();
        plan.TimeFrom.Should().Be(new TimeOnly(8, 0));
        plan.TimeTo.Should().Be(new TimeOnly(10, 0));
    }

    [Theory]
    [InlineData(Periodicity.Monthly,   30)]
    [InlineData(Periodicity.Quarterly, 90)]
    [InlineData(Periodicity.Biannual,  180)]
    [InlineData(Periodicity.Annual,    365)]
    public void Create_DurationDays_se_deriva_de_Periodicity(Periodicity periodicity, int expectedDays)
    {
        var plan = CreateFullFree(periodicity: periodicity);
        plan.DurationDays.Should().Be(expectedDays);
    }

    [Fact]
    public void Create_nombre_se_aplica_trim()
    {
        var plan = MembershipPlan.Create(
            TenantId, "  Plan Test  ", null, Periodicity.Monthly,
            7, false, false, false, false, false, false, false, false,
            true, null, null, 10000m);

        plan.Name.Should().Be("Plan Test");
    }

    // ── Validaciones de negocio ───────────────────────────────────────────

    [Fact]
    public void Create_sin_tenant_lanza_PLAN_TENANT_REQUIRED()
    {
        var act = () => MembershipPlan.Create(
            Guid.Empty, "Plan X", null, Periodicity.Monthly,
            7, false, false, false, false, false, false, false, false,
            true, null, null, 10000m);

        act.Should().Throw<BusinessRuleViolationException>()
           .Where(e => e.Code == "PLAN_TENANT_REQUIRED");
    }

    [Fact]
    public void Create_sin_nombre_lanza_PLAN_NAME_REQUIRED()
    {
        var act = () => MembershipPlan.Create(
            TenantId, "", null, Periodicity.Monthly,
            7, false, false, false, false, false, false, false, false,
            true, null, null, 10000m);

        act.Should().Throw<BusinessRuleViolationException>()
           .Where(e => e.Code == "PLAN_NAME_REQUIRED");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(8)]
    [InlineData(-1)]
    public void Create_daysPerWeek_fuera_de_rango_lanza_excepcion(int days)
    {
        var act = () => MembershipPlan.Create(
            TenantId, "Plan X", null, Periodicity.Monthly,
            days, false, false, false, false, false, false, false, false,
            true, null, null, 10000m);

        act.Should().Throw<BusinessRuleViolationException>()
           .Where(e => e.Code == "PLAN_DAYS_PER_WEEK_INVALID");
    }

    [Fact]
    public void Create_fixedDays_sin_ningún_día_marcado_lanza_PLAN_FIXED_DAYS_REQUIRED()
    {
        var act = () => MembershipPlan.Create(
            TenantId, "Plan X", null, Periodicity.Monthly,
            3, fixedDays: true,
            false, false, false, false, false, false, false, // ningún día marcado
            true, null, null, 10000m);

        act.Should().Throw<BusinessRuleViolationException>()
           .Where(e => e.Code == "PLAN_FIXED_DAYS_REQUIRED");
    }

    [Fact]
    public void Create_fixedDays_dias_marcados_no_coinciden_con_daysPerWeek_lanza_PLAN_DAYS_MISMATCH()
    {
        // DaysPerWeek=3 pero sólo se marcan Lunes y Martes (2 días)
        var act = () => MembershipPlan.Create(
            TenantId, "Plan X", null, Periodicity.Monthly,
            3, fixedDays: true,
            monday: true, tuesday: true, wednesday: false, thursday: false,
            friday: false, saturday: false, sunday: false,
            true, null, null, 10000m);

        act.Should().Throw<BusinessRuleViolationException>()
           .Where(e => e.Code == "PLAN_DAYS_MISMATCH");
    }

    [Fact]
    public void Create_horario_no_libre_sin_timeFrom_lanza_PLAN_TIME_FROM_REQUIRED()
    {
        var act = () => MembershipPlan.Create(
            TenantId, "Plan X", null, Periodicity.Monthly,
            7, false, false, false, false, false, false, false, false,
            freeSchedule: false, timeFrom: null, timeTo: new TimeOnly(10, 0), 10000m);

        act.Should().Throw<BusinessRuleViolationException>()
           .Where(e => e.Code == "PLAN_TIME_FROM_REQUIRED");
    }

    [Fact]
    public void Create_horario_no_libre_sin_timeTo_lanza_PLAN_TIME_TO_REQUIRED()
    {
        var act = () => MembershipPlan.Create(
            TenantId, "Plan X", null, Periodicity.Monthly,
            7, false, false, false, false, false, false, false, false,
            freeSchedule: false, timeFrom: new TimeOnly(8, 0), timeTo: null, 10000m);

        act.Should().Throw<BusinessRuleViolationException>()
           .Where(e => e.Code == "PLAN_TIME_TO_REQUIRED");
    }

    [Fact]
    public void Create_horario_timeFrom_mayor_que_timeTo_lanza_PLAN_TIME_RANGE_INVALID()
    {
        var act = () => MembershipPlan.Create(
            TenantId, "Plan X", null, Periodicity.Monthly,
            7, false, false, false, false, false, false, false, false,
            freeSchedule: false,
            timeFrom: new TimeOnly(12, 0),
            timeTo: new TimeOnly(8, 0),
            10000m);

        act.Should().Throw<BusinessRuleViolationException>()
           .Where(e => e.Code == "PLAN_TIME_RANGE_INVALID");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Create_monto_cero_o_negativo_lanza_PLAN_AMOUNT_INVALID(decimal amount)
    {
        var act = () => CreateFullFree(amount: amount);

        act.Should().Throw<BusinessRuleViolationException>()
           .Where(e => e.Code == "PLAN_AMOUNT_INVALID");
    }

    // ── Desactivación y reactivación ──────────────────────────────────────

    [Fact]
    public void Deactivate_cambia_IsActive_a_false_y_registra_fecha()
    {
        var plan = CreateFullFree();
        var now = DateTime.UtcNow;

        plan.Deactivate(now);

        plan.IsActive.Should().BeFalse();
        plan.DeactivatedAtUtc.Should().Be(now);
    }

    [Fact]
    public void Deactivate_plan_ya_inactivo_lanza_PLAN_ALREADY_INACTIVE()
    {
        var plan = CreateFullFree();
        plan.Deactivate(DateTime.UtcNow);

        var act = () => plan.Deactivate(DateTime.UtcNow);

        act.Should().Throw<BusinessRuleViolationException>()
           .Where(e => e.Code == "PLAN_ALREADY_INACTIVE");
    }

    [Fact]
    public void Reactivate_restaura_IsActive_y_limpia_fecha()
    {
        var plan = CreateFullFree();
        plan.Deactivate(DateTime.UtcNow);

        plan.Reactivate();

        plan.IsActive.Should().BeTrue();
        plan.DeactivatedAtUtc.Should().BeNull();
    }

    // ── Actualización ─────────────────────────────────────────────────────

    [Fact]
    public void Update_recalcula_DurationDays_con_nueva_periodicidad()
    {
        var plan = CreateFullFree(periodicity: Periodicity.Monthly);
        plan.DurationDays.Should().Be(30);

        plan.Update("Plan Anual", null, Periodicity.Annual,
            7, false, false, false, false, false, false, false, false,
            true, null, null, 120000m, false);

        plan.DurationDays.Should().Be(365);
        plan.Amount.Should().Be(120000m);
    }

    [Fact]
    public void Update_con_nombre_vacio_lanza_excepcion()
    {
        var plan = CreateFullFree();

        var act = () => plan.Update("", null, Periodicity.Monthly,
            7, false, false, false, false, false, false, false, false,
            true, null, null, 30000m, false);

        act.Should().Throw<BusinessRuleViolationException>()
           .Where(e => e.Code == "PLAN_NAME_REQUIRED");
    }
}

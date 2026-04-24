using FluentAssertions;
using GymGo.Domain.Exceptions;
using GymGo.Domain.MembershipAssignments;

namespace GymGo.UnitTests.Domain.MembershipAssignments;

public class MembershipAssignmentTests
{
    private static readonly Guid TenantId         = Guid.NewGuid();
    private static readonly Guid MemberId         = Guid.NewGuid();
    private static readonly Guid MembershipPlanId = Guid.NewGuid();

    private static MembershipAssignment CreateValid(
        int durationDays = 30,
        DateOnly? startDate = null) =>
        MembershipAssignment.Create(
            tenantId:         TenantId,
            memberId:         MemberId,
            membershipPlanId: MembershipPlanId,
            planDurationDays: durationDays,
            amountSnapshot:   25000m,
            startDate:        startDate);

    // ── Creación ──────────────────────────────────────────────────────────

    [Fact]
    public void Create_retorna_asignacion_activa_con_pago_pendiente()
    {
        var a = CreateValid();

        a.Id.Should().NotBe(Guid.Empty);
        a.Status.Should().Be(AssignmentStatus.Active);
        a.PaymentStatus.Should().Be(PaymentStatus.Pending);
        a.PaidAtUtc.Should().BeNull();
        a.FrozenSince.Should().BeNull();
        a.FrozenDaysAccumulated.Should().Be(0);
    }

    [Fact]
    public void Create_EndDate_es_StartDate_mas_durationDays()
    {
        var start = new DateOnly(2026, 1, 1);
        var a     = CreateValid(durationDays: 30, startDate: start);

        a.StartDate.Should().Be(start);
        a.EndDate.Should().Be(start.AddDays(30));
    }

    [Fact]
    public void Create_sin_startDate_usa_hoy()
    {
        var a    = CreateValid();
        var hoy  = DateOnly.FromDateTime(DateTime.UtcNow);

        a.StartDate.Should().Be(hoy);
    }

    // ── Validaciones de Create ────────────────────────────────────────────

    [Fact]
    public void Create_sin_tenant_lanza_excepcion()
    {
        var act = () => MembershipAssignment.Create(
            Guid.Empty, MemberId, MembershipPlanId, 30, 25000m);

        act.Should().Throw<BusinessRuleViolationException>()
           .Where(e => e.Code == "ASSIGNMENT_TENANT_REQUIRED");
    }

    [Fact]
    public void Create_sin_member_lanza_excepcion()
    {
        var act = () => MembershipAssignment.Create(
            TenantId, Guid.Empty, MembershipPlanId, 30, 25000m);

        act.Should().Throw<BusinessRuleViolationException>()
           .Where(e => e.Code == "ASSIGNMENT_MEMBER_REQUIRED");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_duracion_invalida_lanza_excepcion(int days)
    {
        var act = () => MembershipAssignment.Create(
            TenantId, MemberId, MembershipPlanId, days, 25000m);

        act.Should().Throw<BusinessRuleViolationException>()
           .Where(e => e.Code == "ASSIGNMENT_DURATION_INVALID");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Create_monto_invalido_lanza_excepcion(decimal amount)
    {
        var act = () => MembershipAssignment.Create(
            TenantId, MemberId, MembershipPlanId, 30, amount);

        act.Should().Throw<BusinessRuleViolationException>()
           .Where(e => e.Code == "ASSIGNMENT_AMOUNT_INVALID");
    }

    // ── Pago ──────────────────────────────────────────────────────────────

    [Fact]
    public void RegisterPayment_cambia_estado_a_Paid_y_registra_fecha()
    {
        var a   = CreateValid();
        var now = DateTime.UtcNow;

        a.RegisterPayment(now);

        a.PaymentStatus.Should().Be(PaymentStatus.Paid);
        a.PaidAtUtc.Should().Be(now);
    }

    [Fact]
    public void RegisterPayment_dos_veces_lanza_ASSIGNMENT_ALREADY_PAID()
    {
        var a = CreateValid();
        a.RegisterPayment(DateTime.UtcNow);

        var act = () => a.RegisterPayment(DateTime.UtcNow);

        act.Should().Throw<BusinessRuleViolationException>()
           .Where(e => e.Code == "ASSIGNMENT_ALREADY_PAID");
    }

    [Fact]
    public void RegisterPayment_en_estado_Overdue_tambien_funciona()
    {
        var a = CreateValid();
        a.MarkOverdue();

        var act = () => a.RegisterPayment(DateTime.UtcNow);

        act.Should().NotThrow();
        a.PaymentStatus.Should().Be(PaymentStatus.Paid);
    }

    // ── Morosidad ─────────────────────────────────────────────────────────

    [Fact]
    public void MarkOverdue_desde_Pending_cambia_a_Overdue()
    {
        var a = CreateValid();

        a.MarkOverdue();

        a.PaymentStatus.Should().Be(PaymentStatus.Overdue);
    }

    [Fact]
    public void MarkOverdue_desde_Paid_lanza_excepcion()
    {
        var a = CreateValid();
        a.RegisterPayment(DateTime.UtcNow);

        var act = () => a.MarkOverdue();

        act.Should().Throw<BusinessRuleViolationException>()
           .Where(e => e.Code == "ASSIGNMENT_OVERDUE_INVALID");
    }

    // ── Cancelación ───────────────────────────────────────────────────────

    [Fact]
    public void Cancel_activa_cambia_a_Cancelled()
    {
        var a = CreateValid();

        a.Cancel();

        a.Status.Should().Be(AssignmentStatus.Cancelled);
    }

    [Fact]
    public void Cancel_vencida_lanza_excepcion()
    {
        var a = CreateValid();
        a.Expire();

        var act = () => a.Cancel();

        act.Should().Throw<BusinessRuleViolationException>()
           .Where(e => e.Code == "ASSIGNMENT_CANCEL_INVALID");
    }

    // ── Congelamiento ─────────────────────────────────────────────────────

    [Fact]
    public void Freeze_activa_cambia_a_Frozen_y_registra_FrozenSince()
    {
        var a     = CreateValid();
        var today = new DateOnly(2026, 3, 1);

        a.Freeze(today);

        a.Status.Should().Be(AssignmentStatus.Frozen);
        a.FrozenSince.Should().Be(today);
    }

    [Fact]
    public void Freeze_ya_congelada_lanza_ASSIGNMENT_ALREADY_FROZEN()
    {
        var a = CreateValid();
        a.Freeze(new DateOnly(2026, 3, 1));

        var act = () => a.Freeze(new DateOnly(2026, 3, 5));

        act.Should().Throw<BusinessRuleViolationException>()
           .Where(e => e.Code == "ASSIGNMENT_ALREADY_FROZEN");
    }

    [Fact]
    public void Unfreeze_extiende_EndDate_por_dias_congelados()
    {
        var start      = new DateOnly(2026, 1, 1);
        var a          = CreateValid(durationDays: 30, startDate: start);
        var endBefore  = a.EndDate; // 2026-01-31

        var freezeDay  = new DateOnly(2026, 1, 10);
        var unfreezeDay = new DateOnly(2026, 1, 20); // 10 días congelado

        a.Freeze(freezeDay);
        a.Unfreeze(unfreezeDay);

        a.Status.Should().Be(AssignmentStatus.Active);
        a.FrozenSince.Should().BeNull();
        a.FrozenDaysAccumulated.Should().Be(10);
        a.EndDate.Should().Be(endBefore.AddDays(10)); // 2026-02-10
    }

    [Fact]
    public void Unfreeze_sin_estar_congelada_lanza_ASSIGNMENT_NOT_FROZEN()
    {
        var a   = CreateValid();
        var act = () => a.Unfreeze(DateOnly.FromDateTime(DateTime.UtcNow));

        act.Should().Throw<BusinessRuleViolationException>()
           .Where(e => e.Code == "ASSIGNMENT_NOT_FROZEN");
    }
}

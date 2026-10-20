using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Cash;
using GymGo.Domain.Exceptions;
using GymGo.Domain.MembershipAssignments;
using GymGo.Domain.MembershipPlans;
using GymGo.Domain.Members;
using GymGo.Domain.Sales;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.Sales.Commands.CheckoutSale;

/// <summary>
/// Handler atómico de venta: crea la Sale, agrega los ítems y el descuento,
/// asigna la membresía (pagada de inmediato), reactiva al socio si estaba
/// moroso, y registra un movimiento de Caja por cada concepto presente en la
/// venta — todo en una sola llamada a SaveChangesAsync. Si algo falla, no
/// queda ningún registro parcial en la base de datos.
/// </summary>
public sealed class CheckoutSaleCommandHandler : IRequestHandler<CheckoutSaleCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _clock;

    public CheckoutSaleCommandHandler(
        IApplicationDbContext context,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser,
        IDateTimeProvider clock)
    {
        _context       = context;
        _currentTenant = currentTenant;
        _currentUser   = currentUser;
        _clock         = clock;
    }

    public async Task<Guid> Handle(CheckoutSaleCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentTenant.TenantId
            ?? throw new UnauthorizedAccessException("No se pudo determinar el tenant actual.");

        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("No se pudo determinar el usuario actual.");

        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == request.MemberId, cancellationToken)
            ?? throw new NotFoundException("Member", request.MemberId);

        // ── 1. Crear la venta y sus ítems ────────────────────────────────
        var sale = Sale.Create(
            tenantId:          tenantId,
            memberId:          member.Id,
            date:              request.Date,
            paymentMethod:     request.PaymentMethod,
            notes:             request.Notes,
            processedByUserId: userId);

        _context.Sales.Add(sale);

        MembershipPlan? plan = null;

        foreach (var itemInput in request.Items)
        {
            if (itemInput.MembershipPlanId.HasValue)
            {
                plan = await _context.MembershipPlans
                    .FirstOrDefaultAsync(p => p.Id == itemInput.MembershipPlanId.Value, cancellationToken)
                    ?? throw new NotFoundException("MembershipPlan", itemInput.MembershipPlanId.Value);

                if (!plan.IsActive)
                    throw new BusinessRuleViolationException(
                        "SALE_PLAN_INACTIVE", "El plan de membresía seleccionado está inactivo.");
            }

            sale.AddItem(
                itemInput.ItemType,
                itemInput.Description,
                itemInput.MembershipPlanId,
                itemInput.UnitPrice,
                itemInput.Quantity);
        }

        // ── 2. Descuento opcional ─────────────────────────────────────────
        if (request.Discount is not null)
        {
            sale.AddDiscount(
                request.Discount.DiscountType,
                request.Discount.Description,
                request.Discount.Value);
        }

        // ── 3. Membresía (si corresponde) ─────────────────────────────────
        var planItem = sale.GetPlanItem();
        MembershipAssignment? assignment = null;

        if (planItem is not null)
        {
            var hasActive = await _context.MembershipAssignments
                .AnyAsync(a =>
                    a.MemberId == sale.MemberId &&
                    (a.Status == AssignmentStatus.Active || a.Status == AssignmentStatus.Frozen),
                    cancellationToken);

            if (hasActive)
                throw new BusinessRuleViolationException(
                    "SALE_MEMBER_ALREADY_ACTIVE",
                    "El socio ya tiene una membresía activa o congelada. Cancélela antes de confirmar esta venta.");

            plan ??= await _context.MembershipPlans
                .FirstOrDefaultAsync(p => p.Id == planItem.MembershipPlanId!.Value, cancellationToken)
                ?? throw new NotFoundException("MembershipPlan", planItem.MembershipPlanId!.Value);

            assignment = MembershipAssignment.Create(
                tenantId:         tenantId,
                memberId:         sale.MemberId,
                membershipPlanId: planItem.MembershipPlanId!.Value,
                planDurationDays: plan.DurationDays,
                amountSnapshot:   planItem.UnitPrice,
                startDate:        sale.Date,
                saleId:           sale.Id,
                notes:            null);

            // El pago se recibe en el mismo acto de Cobrar (ver paso 4), así que
            // la membresía queda pagada de inmediato en vez de "Pendiente".
            assignment.RegisterPayment(_clock.UtcNow);

            _context.MembershipAssignments.Add(assignment);
        }

        // Si el socio estaba moroso y acaba de pagar, se reactiva.
        if (member.Status == MemberStatus.Delinquent)
            member.Activate();

        // ── 4. Caja: un movimiento por cada concepto presente en la venta ─
        var cashTransactions = BuildCashTransactions(sale, assignment, tenantId, userId, _clock.UtcNow);
        _context.CashTransactions.AddRange(cashTransactions);

        var primaryCashTx = cashTransactions.FirstOrDefault(t => t.Concept == TransactionConcept.CuotaMembresia)
            ?? cashTransactions[0];

        sale.Confirm(assignment?.Id, primaryCashTx.Id);

        // ── 5. Persistir todo en una sola transacción ────────────────────
        await _context.SaveChangesAsync(cancellationToken);

        return sale.Id;
    }

    private static readonly SaleItemType[] ConceptOrder =
    [
        SaleItemType.Plan,
        SaleItemType.Matricula,
        SaleItemType.ServicioExtra,
        SaleItemType.Otro,
    ];

    private static TransactionConcept ToConcept(SaleItemType itemType) => itemType switch
    {
        SaleItemType.Plan          => TransactionConcept.CuotaMembresia,
        SaleItemType.Matricula     => TransactionConcept.Matricula,
        SaleItemType.ServicioExtra => TransactionConcept.ProductoServicio,
        SaleItemType.Otro          => TransactionConcept.OtroIngreso,
        _ => throw new ArgumentOutOfRangeException(nameof(itemType)),
    };

    /// <summary>
    /// Agrupa los ítems de la venta por tipo (→ concepto de Caja) y crea un
    /// CashTransaction por grupo, prorrateando el descuento total de la venta
    /// proporcionalmente al peso de cada grupo sobre el subtotal. Solo el
    /// grupo de tipo Plan queda enlazado a la MembershipAssignment. Si el
    /// descuento deja todos los grupos en $0 (venta 100% cortesía), se crea
    /// un único movimiento simbólico de $0.01 en vez de no crear ninguno.
    /// </summary>
    private static List<CashTransaction> BuildCashTransactions(
        Sale sale, MembershipAssignment? assignment,
        Guid tenantId, Guid userId, DateTime nowUtc)
    {
        var groups = ConceptOrder
            .Select(type => new
            {
                Type  = type,
                Items = sale.Items.Where(i => i.ItemType == type).ToList(),
            })
            .Where(g => g.Items.Count > 0)
            .Select(g => new { g.Type, g.Items, Subtotal = g.Items.Sum(i => i.Subtotal) })
            .ToList();

        var transactions = new List<CashTransaction>();
        var remainingDiscount = sale.DiscountAmount;

        for (var i = 0; i < groups.Count; i++)
        {
            var group  = groups[i];
            var isLast = i == groups.Count - 1;

            var groupDiscount = isLast
                ? remainingDiscount
                : Math.Round(sale.DiscountAmount * group.Subtotal / sale.SubtotalAmount, 2);

            remainingDiscount -= groupDiscount;

            var amount = group.Subtotal - groupDiscount;
            if (amount <= 0) continue;

            var concept = ToConcept(group.Type);

            transactions.Add(CashTransaction.Create(
                tenantId:               tenantId,
                date:                   sale.Date,
                type:                   CashTransactionType.Ingreso,
                amount:                 amount,
                paymentMethod:          sale.PaymentMethod,
                concept:                concept,
                description:            BuildCashDescription(group.Items, groupDiscount),
                memberId:               sale.MemberId,
                membershipAssignmentId: concept == TransactionConcept.CuotaMembresia ? assignment?.Id : null,
                processedByUserId:      userId,
                createdAtUtc:           nowUtc,
                saleId:                 sale.Id));
        }

        if (transactions.Count == 0)
        {
            transactions.Add(CashTransaction.Create(
                tenantId:               tenantId,
                date:                   sale.Date,
                type:                   CashTransactionType.Ingreso,
                amount:                 0.01m,
                paymentMethod:          sale.PaymentMethod,
                concept:                TransactionConcept.OtroIngreso,
                description:            BuildCashDescription(sale.Items.ToList(), 0) + " (cortesía)",
                memberId:               sale.MemberId,
                membershipAssignmentId: null,
                processedByUserId:      userId,
                createdAtUtc:           nowUtc,
                saleId:                 sale.Id));
        }

        return transactions;
    }

    private static string BuildCashDescription(IReadOnlyList<SaleItem> items, decimal discountForGroup)
    {
        var desc = string.Join(", ", items.Select(i => i.Description));

        if (discountForGroup > 0)
            desc += $" (Desc: -{discountForGroup:N0})";

        return desc.Length > 500 ? desc[..500] : desc;
    }
}

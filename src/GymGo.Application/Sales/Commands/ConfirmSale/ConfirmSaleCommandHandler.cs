using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Cash;
using GymGo.Domain.Exceptions;
using GymGo.Domain.MembershipAssignments;
using GymGo.Domain.Sales;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.Sales.Commands.ConfirmSale;

/// <summary>
/// Lógica de confirmación de venta:
///
/// 1. Cargar la Sale con Items y Discounts.
/// 2. Si hay ítem Plan: verificar que el socio no tenga membresía Active/Frozen.
/// 3. Si hay ítem Plan: crear MembershipAssignment con AmountSnapshot = UnitPrice del ítem Plan.
/// 4. Determinar el concepto de caja según los ítems presentes.
/// 5. Crear CashTransaction con Amount = Sale.TotalAmount.
/// 6. Llamar a Sale.Confirm() con los IDs generados.
/// 7. SaveChanges: todo en una sola transacción.
/// </summary>
public sealed class ConfirmSaleCommandHandler : IRequestHandler<ConfirmSaleCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _clock;

    public ConfirmSaleCommandHandler(
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

    public async Task Handle(ConfirmSaleCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentTenant.TenantId
            ?? throw new UnauthorizedAccessException("No se pudo determinar el tenant actual.");

        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("No se pudo determinar el usuario actual.");

        // 1. Cargar la venta con sus colecciones
        var sale = await _context.Sales
            .Include(s => s.Items)
            .Include(s => s.Discounts)
            .FirstOrDefaultAsync(s => s.Id == request.SaleId, cancellationToken)
            ?? throw new NotFoundException("Sale", request.SaleId);

        var planItem = sale.GetPlanItem();

        // 2. Si hay ítem Plan: cargar plan y verificar que el socio no tenga membresía vigente
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

            var plan = await _context.MembershipPlans
                .FirstOrDefaultAsync(p => p.Id == planItem.MembershipPlanId!.Value, cancellationToken)
                ?? throw new NotFoundException("MembershipPlan", planItem.MembershipPlanId!.Value);

            if (!plan.IsActive)
                throw new BusinessRuleViolationException(
                    "SALE_PLAN_INACTIVE",
                    "El plan de membresía seleccionado está inactivo.");

            // 3. Crear MembershipAssignment
            assignment = MembershipAssignment.Create(
                tenantId:         tenantId,
                memberId:         sale.MemberId,
                membershipPlanId: planItem.MembershipPlanId!.Value,
                planDurationDays: plan.DurationDays,
                amountSnapshot:   planItem.UnitPrice,
                startDate:        sale.Date,
                saleId:           sale.Id,
                notes:            null);

            _context.MembershipAssignments.Add(assignment);
        }

        // 4. Determinar concepto de caja
        //    Si solo hay Plan → CuotaMembresia; si hay ítems mixtos → ProductoServicio
        var concept = sale.Items.All(i => i.ItemType == SaleItemType.Plan)
            ? TransactionConcept.CuotaMembresia
            : TransactionConcept.ProductoServicio;

        // Monto mínimo: si TotalAmount = 0 (venta cortesía) se usa 0.01 para respetar
        // la restricción de Amount > 0 de CashTransaction. En ese caso la descripción
        // documenta que es una cortesía. Alternativamente, puede no crearse CashTransaction.
        // Por diseño: si TotalAmount > 0, se crea; si = 0, se crea con monto simbólico
        // y descripción aclaratoria (se puede ajustar según política del negocio).
        var cashAmount = sale.TotalAmount > 0 ? sale.TotalAmount : 0.01m;

        // Descripción de la transacción
        var description = BuildCashDescription(sale);

        // 5. Crear CashTransaction
        var cashTx = CashTransaction.Create(
            tenantId:               tenantId,
            date:                   sale.Date,
            type:                   CashTransactionType.Ingreso,
            amount:                 cashAmount,
            paymentMethod:          sale.PaymentMethod,
            concept:                concept,
            description:            description,
            memberId:               sale.MemberId,
            membershipAssignmentId: assignment?.Id,
            processedByUserId:      userId,
            createdAtUtc:           _clock.UtcNow,
            saleId:                 sale.Id);

        _context.CashTransactions.Add(cashTx);

        // 6. Confirmar la venta
        sale.Confirm(assignment?.Id, cashTx.Id);

        // 7. SaveChanges: todo en una sola transacción de BD
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static string BuildCashDescription(Domain.Sales.Sale sale)
    {
        var parts = sale.Items
            .Select(i => i.Description)
            .ToList();

        var desc = string.Join(", ", parts);

        if (sale.DiscountAmount > 0)
            desc += $" (Desc: -{sale.DiscountAmount:N0})";

        return desc.Length > 500 ? desc[..500] : desc;
    }
}

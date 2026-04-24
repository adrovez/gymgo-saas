using GymGo.Domain.Members;
using GymGo.Domain.MembershipAssignments;
using GymGo.Domain.MembershipPlans;
using GymGo.Domain.Tenants;
using GymGo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.Common.Interfaces;

/// <summary>
/// Contrato del DbContext expuesto a la capa Application para evitar
/// acoplarla a EF Core concreto. Implementado por ApplicationDbContext.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<User> Users { get; }
    DbSet<Member> Members { get; }
    DbSet<MembershipPlan> MembershipPlans { get; }
    DbSet<MembershipAssignment> MembershipAssignments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

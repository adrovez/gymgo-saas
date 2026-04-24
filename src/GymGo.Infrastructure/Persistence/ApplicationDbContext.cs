using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Members;
using GymGo.Domain.MembershipAssignments;
using GymGo.Domain.MembershipPlans;
using GymGo.Domain.Tenants;
using GymGo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Infrastructure.Persistence;

public sealed class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentTenant _currentTenant;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentTenant currentTenant)
        : base(options)
    {
        _currentTenant = currentTenant;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<MembershipPlan> MembershipPlans => Set<MembershipPlan>();
    public DbSet<MembershipAssignment> MembershipAssignments => Set<MembershipAssignment>();

    /// <summary>
    /// EF Core trata los accesos a propiedades de instancia del DbContext
    /// dentro de query filters como "client-bound parameters": el modelo
    /// se cachea a nivel de la shape pero el valor se evalúa por instancia
    /// en cada query. Esto permite que HasQueryFilter respete el tenant
    /// actual del request en curso.
    /// </summary>
    public Guid CurrentTenantIdOrEmpty => _currentTenant.TenantId ?? Guid.Empty;
    public bool CurrentHasTenant => _currentTenant.HasTenant;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // ── Multi-tenancy + soft delete ──────────────────────────────────
        // Filtros de consulta explícitos por entidad. EF Core los parametriza
        // por instancia del DbContext (no captura "this" estáticamente).
        //
        // Tenants: NO es ITenantScoped (es la tabla padre del aislamiento),
        // por eso no lleva filtro.
        //
        // Users: ITenantScoped + ISoftDeletable.
        modelBuilder.Entity<User>().HasQueryFilter(u =>
            (!CurrentHasTenant || u.TenantId == CurrentTenantIdOrEmpty)
            && !u.IsDeleted);

        // Members: ITenantScoped + ISoftDeletable.
        modelBuilder.Entity<Member>().HasQueryFilter(m =>
            (!CurrentHasTenant || m.TenantId == CurrentTenantIdOrEmpty)
            && !m.IsDeleted);

        // MembershipPlans: ITenantScoped + ISoftDeletable.
        modelBuilder.Entity<MembershipPlan>().HasQueryFilter(p =>
            (!CurrentHasTenant || p.TenantId == CurrentTenantIdOrEmpty)
            && !p.IsDeleted);

        // MembershipAssignments: ITenantScoped + ISoftDeletable.
        modelBuilder.Entity<MembershipAssignment>().HasQueryFilter(a =>
            (!CurrentHasTenant || a.TenantId == CurrentTenantIdOrEmpty)
            && !a.IsDeleted);
    }
}

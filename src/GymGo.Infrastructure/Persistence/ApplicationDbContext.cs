using GymGo.Application.Common.Interfaces;
using GymGo.Domain.ClassAttendances;
using GymGo.Domain.ClassReservations;
using GymGo.Domain.Equipments;
using GymGo.Domain.GymClasses;
using GymGo.Domain.GymEntries;
using GymGo.Domain.Maintenance;
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
    public DbSet<GymClass> GymClasses => Set<GymClass>();
    public DbSet<ClassSchedule> ClassSchedules => Set<ClassSchedule>();
    public DbSet<ClassAttendance> ClassAttendances => Set<ClassAttendance>();
    public DbSet<GymEntry> GymEntries => Set<GymEntry>();
    public DbSet<ClassReservation> ClassReservations => Set<ClassReservation>();
    public DbSet<Equipment> Equipment => Set<Equipment>();
    public DbSet<MaintenanceRecord> MaintenanceRecords => Set<MaintenanceRecord>();

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

        // GymClasses: ITenantScoped + ISoftDeletable.
        modelBuilder.Entity<GymClass>().HasQueryFilter(c =>
            (!CurrentHasTenant || c.TenantId == CurrentTenantIdOrEmpty)
            && !c.IsDeleted);

        // ClassSchedules: ITenantScoped + ISoftDeletable.
        modelBuilder.Entity<ClassSchedule>().HasQueryFilter(s =>
            (!CurrentHasTenant || s.TenantId == CurrentTenantIdOrEmpty)
            && !s.IsDeleted);

        // ClassAttendances: ITenantScoped (sin soft delete — los registros son inmutables).
        modelBuilder.Entity<ClassAttendance>().HasQueryFilter(a =>
            !CurrentHasTenant || a.TenantId == CurrentTenantIdOrEmpty);

        // GymEntries: ITenantScoped (sin soft delete — los ingresos son inmutables).
        modelBuilder.Entity<GymEntry>().HasQueryFilter(e =>
            !CurrentHasTenant || e.TenantId == CurrentTenantIdOrEmpty);

        // ClassReservations: ITenantScoped (sin soft delete — las reservas son registros de auditoría).
        modelBuilder.Entity<ClassReservation>().HasQueryFilter(r =>
            !CurrentHasTenant || r.TenantId == CurrentTenantIdOrEmpty);

        // Equipment: ITenantScoped + ISoftDeletable.
        modelBuilder.Entity<Equipment>().HasQueryFilter(e =>
            (!CurrentHasTenant || e.TenantId == CurrentTenantIdOrEmpty)
            && !e.IsDeleted);

        // MaintenanceRecords: ITenantScoped (sin soft delete — son registros de auditoría).
        modelBuilder.Entity<MaintenanceRecord>().HasQueryFilter(m =>
            !CurrentHasTenant || m.TenantId == CurrentTenantIdOrEmpty);
    }
}

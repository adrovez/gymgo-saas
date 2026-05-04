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
using GymGo.Domain.WorkoutLogs;
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
    DbSet<GymClass> GymClasses { get; }
    DbSet<ClassSchedule> ClassSchedules { get; }
    DbSet<ClassAttendance> ClassAttendances { get; }
    DbSet<GymEntry> GymEntries { get; }
    DbSet<ClassReservation> ClassReservations { get; }
    DbSet<GymGo.Domain.Equipments.Equipment> Equipment { get; }
    DbSet<MaintenanceRecord> MaintenanceRecords { get; }
    DbSet<WorkoutLog> WorkoutLogs { get; }
    DbSet<WorkoutLogExercise> WorkoutLogExercises { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

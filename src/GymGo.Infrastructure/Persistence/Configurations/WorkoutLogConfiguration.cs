using GymGo.Domain.WorkoutLogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymGo.Infrastructure.Persistence.Configurations;

public sealed class WorkoutLogConfiguration : IEntityTypeConfiguration<WorkoutLog>
{
    public void Configure(EntityTypeBuilder<WorkoutLog> builder)
    {
        builder.ToTable("WorkoutLogs");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.TenantId).IsRequired();
        builder.Property(w => w.MemberId).IsRequired();

        builder.HasOne<Domain.Members.Member>()
            .WithMany()
            .HasForeignKey(w => w.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(w => w.WorkoutPlanId).IsRequired();
        builder.Property(w => w.WorkoutPlanDayId).IsRequired();

        builder.HasOne<WorkoutPlan>()
            .WithMany()
            .HasForeignKey(w => w.WorkoutPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<WorkoutPlanDay>()
            .WithMany()
            .HasForeignKey(w => w.WorkoutPlanDayId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(w => w.Date).IsRequired().HasColumnType("date");
        builder.Property(w => w.Notes).HasMaxLength(1000);

        builder.Property(w => w.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(WorkoutLogStatus.Draft);

        builder.HasMany(w => w.Exercises)
            .WithOne()
            .HasForeignKey(e => e.WorkoutLogId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(w => new { w.TenantId, w.MemberId, w.Date })
            .HasDatabaseName("IX_WorkoutLogs_Member_Date");

        builder.HasIndex(w => w.WorkoutPlanId)
            .HasDatabaseName("IX_WorkoutLogs_WorkoutPlanId");

        builder.Property(w => w.CreatedAtUtc).IsRequired();
        builder.Property(w => w.CreatedBy).HasMaxLength(200);
        builder.Property(w => w.ModifiedBy).HasMaxLength(200);

        builder.Property(w => w.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(w => w.DeletedBy).HasMaxLength(200);

        builder.Ignore(w => w.DomainEvents);
    }
}

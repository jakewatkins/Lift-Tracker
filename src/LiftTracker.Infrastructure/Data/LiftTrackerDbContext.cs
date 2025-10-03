using Microsoft.EntityFrameworkCore;
using LiftTracker.Domain.Entities;

namespace LiftTracker.Infrastructure.Data;

/// <summary>
/// Entity Framework Core database context for the Lift Tracker application
/// </summary>
public class LiftTrackerDbContext : DbContext
{
    public LiftTrackerDbContext(DbContextOptions<LiftTrackerDbContext> options) : base(options)
    {
    }

    // Entity sets
    public DbSet<User> Users { get; set; }
    public DbSet<WorkoutSession> WorkoutSessions { get; set; }
    public DbSet<ExerciseType> ExerciseTypes { get; set; }
    public DbSet<StrengthLift> StrengthLifts { get; set; }
    public DbSet<MetconType> MetconTypes { get; set; }
    public DbSet<MetconWorkout> MetconWorkouts { get; set; }
    public DbSet<MovementType> MovementTypes { get; set; }
    public DbSet<MetconMovement> MetconMovements { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(254);
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.CreatedDate)
                .IsRequired();
            entity.Property(e => e.LastLoginDate);

            // Indexes
            entity.HasIndex(e => e.Email)
                .IsUnique();
        });

        // Configure WorkoutSession entity
        modelBuilder.Entity<WorkoutSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId)
                .IsRequired();
            entity.Property(e => e.Date)
                .IsRequired();
            entity.Property(e => e.Notes)
                .HasMaxLength(1000);

            // Relationships
            entity.HasOne(e => e.User)
                .WithMany(e => e.WorkoutSessions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes and constraints
            entity.HasIndex(e => new { e.UserId, e.Date })
                .IsUnique();
            entity.HasIndex(e => new { e.UserId, e.Date })
                .HasDatabaseName("IX_WorkoutSession_UserId_Date");
        });

        // Configure ExerciseType entity
        modelBuilder.Entity<ExerciseType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.Category)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Constraints
            entity.HasIndex(e => e.Name)
                .IsUnique();
        });

        // Configure StrengthLift entity
        modelBuilder.Entity<StrengthLift>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.WorkoutSessionId)
                .IsRequired();
            entity.Property(e => e.ExerciseTypeId)
                .IsRequired();
            entity.Property(e => e.SetStructure)
                .IsRequired()
                .HasMaxLength(20);
            entity.Property(e => e.Sets);
            entity.Property(e => e.Reps);
            entity.Property(e => e.Weight)
                .IsRequired()
                .HasColumnType("decimal(5,2)");
            entity.Property(e => e.AdditionalWeight)
                .HasColumnType("decimal(5,2)");
            entity.Property(e => e.Duration)
                .HasColumnType("decimal(4,2)");
            entity.Property(e => e.RestPeriod)
                .HasColumnType("decimal(4,2)");
            entity.Property(e => e.Comments)
                .HasMaxLength(500);
            entity.Property(e => e.Order)
                .IsRequired();

            // Relationships
            entity.HasOne(e => e.WorkoutSession)
                .WithMany(e => e.StrengthLifts)
                .HasForeignKey(e => e.WorkoutSessionId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.ExerciseType)
                .WithMany(e => e.StrengthLifts)
                .HasForeignKey(e => e.ExerciseTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            entity.HasIndex(e => new { e.WorkoutSessionId, e.Order })
                .HasDatabaseName("IX_StrengthLift_WorkoutSessionId_Order");
        });

        // Configure MetconType entity
        modelBuilder.Entity<MetconType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.Description)
                .HasMaxLength(200);
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Constraints
            entity.HasIndex(e => e.Name)
                .IsUnique();
        });

        // Configure MetconWorkout entity
        modelBuilder.Entity<MetconWorkout>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.WorkoutSessionId)
                .IsRequired();
            entity.Property(e => e.MetconTypeId)
                .IsRequired();
            entity.Property(e => e.TotalTime)
                .HasColumnType("decimal(6,2)");
            entity.Property(e => e.RoundsCompleted);
            entity.Property(e => e.Notes)
                .HasMaxLength(1000);
            entity.Property(e => e.Order)
                .IsRequired();

            // Relationships
            entity.HasOne(e => e.WorkoutSession)
                .WithMany(e => e.MetconWorkouts)
                .HasForeignKey(e => e.WorkoutSessionId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.MetconType)
                .WithMany(e => e.MetconWorkouts)
                .HasForeignKey(e => e.MetconTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            entity.HasIndex(e => new { e.WorkoutSessionId, e.Order })
                .HasDatabaseName("IX_MetconWorkout_WorkoutSessionId_Order");
        });

        // Configure MovementType entity
        modelBuilder.Entity<MovementType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.Category)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.MeasurementType)
                .IsRequired()
                .HasMaxLength(20);
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Constraints
            entity.HasIndex(e => e.Name)
                .IsUnique();
        });

        // Configure MetconMovement entity
        modelBuilder.Entity<MetconMovement>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MetconWorkoutId)
                .IsRequired();
            entity.Property(e => e.MovementTypeId)
                .IsRequired();
            entity.Property(e => e.Reps);
            entity.Property(e => e.Distance)
                .HasColumnType("decimal(8,2)");
            entity.Property(e => e.Weight)
                .HasColumnType("decimal(5,2)");
            entity.Property(e => e.Order)
                .IsRequired();

            // Relationships
            entity.HasOne(e => e.MetconWorkout)
                .WithMany(e => e.MetconMovements)
                .HasForeignKey(e => e.MetconWorkoutId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.MovementType)
                .WithMany(e => e.MetconMovements)
                .HasForeignKey(e => e.MovementTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            entity.HasIndex(e => new { e.MetconWorkoutId, e.Order })
                .HasDatabaseName("IX_MetconMovement_MetconWorkoutId_Order");
        });

        // Seed data
        modelBuilder.Entity<ExerciseType>().HasData(SeedData.ExerciseTypes);
        modelBuilder.Entity<MetconType>().HasData(SeedData.MetconTypes);
        modelBuilder.Entity<MovementType>().HasData(SeedData.MovementTypes);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Philosophers.DB.Entities;

namespace Philosophers.DB;

public class RunsContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Runs> Runs { get; set; } = null!;
    public DbSet<PhilosophersEntity> Philosophers { get; set; } = null!;
    public DbSet<ForksEntity> Forks { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // +================+
        // | Table settings |
        // +================+

        // Runs settings
        modelBuilder.Entity<Runs>().ToTable("simulation_runs");
        modelBuilder.Entity<Runs>().HasKey(r => r.RunId);

        // Philosophers settings
        modelBuilder.Entity<PhilosophersEntity>().ToTable("philosophers");
        modelBuilder.Entity<PhilosophersEntity>().HasKey(p => p.PhilosophersEntityId);
        modelBuilder.Entity<PhilosophersEntity>()
            .HasOne(p => p.Run)
            .WithMany(r => r.Philosophers)
            .HasForeignKey(p => p.RunId);

        // Forks settings
        modelBuilder.Entity<ForksEntity>().ToTable("forks");
        modelBuilder.Entity<ForksEntity>().HasKey(p => p.ForksEntityId);
        modelBuilder.Entity<ForksEntity>()
            .HasOne(r => r.Philosopher)
            .WithMany(p => p.Forks)
            .HasForeignKey(r => r.PhilosopherId);

        // +=========+
        // | Indexes |
        // +=========+

        // Runs indexes settings
        modelBuilder.Entity<Runs>().HasIndex(r => r.Step).HasDatabaseName("idx_step");
        modelBuilder.Entity<Runs>().HasIndex(r => r.SimulationState).HasDatabaseName("idx_sim_state");

        // +==================+
        // | Columns settings |
        // +==================+

        // Runs columns settings
        modelBuilder.Entity<Runs>().Property(r => r.RunId).HasColumnName("run_id");
        modelBuilder.Entity<Runs>().Property(r => r.Step).HasColumnName("step");
        modelBuilder.Entity<Runs>().Property(r => r.Duration).HasColumnName("duration");
        modelBuilder.Entity<Runs>().Property(r => r.SimulationState).HasColumnName("simulation_state");

        modelBuilder.Entity<Runs>().Property(r => r.SimulationState).HasConversion<string>();

        // Philosophers columns settings
        modelBuilder.Entity<PhilosophersEntity>().Property(r => r.PhilosophersEntityId).HasColumnName("philosopher_id");
        modelBuilder.Entity<PhilosophersEntity>().Property(r => r.PhilosopherState).HasColumnName("philosopher_state");
        modelBuilder.Entity<PhilosophersEntity>().Property(r => r.RunId).HasColumnName("run_id");

        // Forks column settings
        modelBuilder.Entity<ForksEntity>().Property(r => r.ForksEntityId).HasColumnName("fork_id");
        modelBuilder.Entity<ForksEntity>().Property(r => r.ForkState).HasColumnName("fork_state");
        modelBuilder.Entity<ForksEntity>().Property(r => r.PhilosopherId).HasColumnName("philosopher_id");
    }
}

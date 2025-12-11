using Microsoft.EntityFrameworkCore;

namespace DbLogger
{
    public class DbLoggerContext : DbContext
    {
        public DbLoggerContext(DbContextOptions<DbLoggerContext> options)
            : base(options)
        {
        }

        public DbSet<RunSnapshot> Runs => Set<RunSnapshot>();
        public DbSet<TableStateSnapshot> TableStates => Set<TableStateSnapshot>();
        public DbSet<PhilosopherStateSnapshot> Philosophers => Set<PhilosopherStateSnapshot>();
        public DbSet<ForkStateSnapshot> Forks => Set<ForkStateSnapshot>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RunSnapshot>()
                .HasMany(r => r.Steps)
                .WithOne(t => t.RunSnapshot)
                .HasForeignKey(t => t.RunSnapshotId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TableStateSnapshot>()
                .HasMany(t => t.Philosophers)
                .WithOne(p => p.TableStateSnapshot)
                .HasForeignKey(p => p.TableStateSnapshotId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TableStateSnapshot>()
                .HasMany(t => t.Forks)
                .WithOne(f => f.TableStateSnapshot)
                .HasForeignKey(f => f.TableStateSnapshotId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}


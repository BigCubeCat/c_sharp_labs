using System;
using DbLogger.Models;
using Microsoft.EntityFrameworkCore;

namespace DbLogger.Db
{
    public class DbLoggerContext : DbContext
    {
        public DbSet<Stage> Stages => Set<Stage>();
        public DbSet<PhilosopherEntity> PhilosopherEntities => Set<PhilosopherEntity>();
        public DbSet<ForkEntity> ForkEntities => Set<ForkEntity>();
        public DbSet<PhilosopherEntityState> PhilosopherEntityStates => Set<PhilosopherEntityState>();
        public DbSet<ForkEntityState> ForkEntityStates => Set<ForkEntityState>();
        public DbSet<TimeStamp> TimeStamps => Set<TimeStamp>();

        public DbLoggerContext(DbContextOptions<DbLoggerContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ключи и связи
            modelBuilder.Entity<Stage>()
                .HasMany(s => s.Philosophers)
                .WithOne(p => p.Stage)
                .HasForeignKey(p => p.StageId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Stage>()
                .HasMany(s => s.Forks)
                .WithOne(f => f.Stage)
                .HasForeignKey(f => f.StageId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Stage>()
                .HasMany(s => s.TimeStamps)
                .WithOne(t => t.Stage)
                .HasForeignKey(t => t.StageId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PhilosopherEntity>()
                .HasMany(p => p.States)
                .WithOne(s => s.Philosopher)
                .HasForeignKey(s => s.PhilosopherId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ForkEntity>()
                .HasMany(f => f.States)
                .WithOne(s => s.Fork)
                .HasForeignKey(s => s.ForkId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TimeStamp>()
                .HasMany(t => t.PhilosopherStates)
                .WithOne(s => s.TimeStamp)
                .HasForeignKey(s => s.TimeStampId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TimeStamp>()
                .HasMany(t => t.ForkStates)
                .WithOne(s => s.TimeStamp)
                .HasForeignKey(s => s.TimeStampId)
                .OnDelete(DeleteBehavior.Cascade);

            // Внешняя ключ-навигция для OwnerPhilosopher в ForkEntityState (опционально)
            modelBuilder.Entity<ForkEntityState>()
                .HasOne(s => s.OwnerPhilosopher)
                .WithMany()
                .HasForeignKey(s => s.OwnerPhilosopherId)
                .OnDelete(DeleteBehavior.SetNull);

            // Seed: один Stage с 5 философами и 5 вилками
            var stageId = 1;
            var stageSeed = new Stage
            {
                Id = stageId,
                Name = "DefaultStage",
                CreatedAtUtc = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc)
            };
            modelBuilder.Entity<Stage>().HasData(stageSeed);

            // Philosophers (Id: 1..5)
            for (int i = 1; i <= 5; i++)
            {
                modelBuilder.Entity<PhilosopherEntity>().HasData(new PhilosopherEntity
                {
                    Id = i,
                    Name = $"Philosopher {i}",
                    StageId = stageId
                });
            }

            // Forks (Id: 1..5)
            for (int i = 1; i <= 5; i++)
            {
                modelBuilder.Entity<ForkEntity>().HasData(new ForkEntity
                {
                    Id = 100 + i, // используем отдельный id диапазон, чтобы избежать пересечений при автогенерации
                    Number = i,
                    StageId = stageId
                });
            }

            // Примечание: состояния (TimeStamp и State-ы) по умолчанию не засеваются.
        }
    }
}


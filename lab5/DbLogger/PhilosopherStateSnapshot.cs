using System.ComponentModel.DataAnnotations;

namespace DbLogger
{
    public class PhilosopherStateSnapshot
    {
        public int Id { get; set; }

        [Required]
        public string PhilosopherName { get; set; } = string.Empty;

        [Required]
        public string State { get; set; } = string.Empty; // Thinking, Hungry, Eating

        public double Throughput { get; set; }
        public double HungryPercentage { get; set; }
        public int MealsEaten { get; set; }

        public int TableStateSnapshotId { get; set; }
        public TableStateSnapshot? TableStateSnapshot { get; set; }
    }
}


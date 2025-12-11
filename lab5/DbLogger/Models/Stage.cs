using System;
using System.Collections.Generic;

namespace DbLogger.Models
{
    public class Stage
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime CreatedAtUtc { get; set; }

        public ICollection<PhilosopherEntity> Philosophers { get; set; } = new List<PhilosopherEntity>();
        public ICollection<ForkEntity> Forks { get; set; } = new List<ForkEntity>();
        public ICollection<TimeStamp> TimeStamps { get; set; } = new List<TimeStamp>();
    }
}


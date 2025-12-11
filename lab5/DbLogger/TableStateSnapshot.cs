using System;
using System.Collections.Generic;

namespace DbLogger
{
    public class TableStateSnapshot
    {
        public int Id { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public int RunSnapshotId { get; set; }
        public RunSnapshot? RunSnapshot { get; set; }

        public List<PhilosopherStateSnapshot> Philosophers { get; set; } = new();
        public List<ForkStateSnapshot> Forks { get; set; } = new();
    }
}


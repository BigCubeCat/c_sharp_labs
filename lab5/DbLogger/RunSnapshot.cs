using System;
using System.Collections.Generic;

namespace DbLogger
{
    public class RunSnapshot
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<TableStateSnapshot> Steps { get; set; } = new();
    }
}

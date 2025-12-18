using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Philosophers.DB.Entities;

public class PhilosophersEntity
{
    public int PhilosophersEntityId { get; set; }
    public required string PhilosopherState { get; set; }

    public int? RunId { get; set; }
    public Runs? Run { get; set; }

    public required IList<ForksEntity> Forks { get; set; }
}

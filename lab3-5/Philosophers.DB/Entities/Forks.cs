using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Philosophers.DB.Entities;

public class ForksEntity
{
    public int ForksEntityId { get; set; }
    public required string ForkState { get; set; }
    
    public int? PhilosopherId { get; set; }
    public PhilosophersEntity? Philosopher { get; set; }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Interface.DTO;

public record PhilosophersDto(string PhilosopherState, ForksDto LeftFork, ForksDto RightFork);

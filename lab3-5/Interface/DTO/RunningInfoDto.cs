using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Interface.DTO;

public record RunningInfoDto(int? Id, int Step, long Duration,
    SimulationStates SimulationState, IList<PhilosophersDto> Philosophers);
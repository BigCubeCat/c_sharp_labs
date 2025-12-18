using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Interface.DTO;

namespace Interface;

public interface ISimulationDatabaseProcessor
{
    Task SaveRunningInfoAsync(RunningInfoDto runningInfoDto, CancellationToken ct);
    Task<RunningInfoDto> GetRunningInfoByIdAsync(int id, CancellationToken ct);
    Task<IList<RunningInfoDto>> GetRunningInfoBySimulationStateAsync(SimulationStates state, CancellationToken ct);
    Task<IList<RunningInfoDto>> GetRunningInfoByStepAsync(int step, CancellationToken ct);
    Task<IList<RunningInfoDto>> GetAllRunningInfosAsync(CancellationToken ct);
}

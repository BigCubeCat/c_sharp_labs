using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Interface;
using Microsoft.Extensions.Logging;
using Philosophers.DB;
using Philosophers.DB.Entities;
using Microsoft.EntityFrameworkCore;
using Interface.DTO;

namespace Philosophers.Services.DB.Service;

public class SimulationDatabaseProcessor(RunsContext db, ILogger<SimulationDatabaseProcessor> logger):
    ISimulationDatabaseProcessor
{
    private readonly RunsContext _db = db;
    private readonly ILogger<SimulationDatabaseProcessor> _logger = logger;

    public static SimulationDatabaseProcessor Create(string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<RunsContext>();
        optionsBuilder.UseNpgsql(connectionString);

        var dbContext = new RunsContext(optionsBuilder.Options);
        var loggersFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddConsole()
                .SetMinimumLevel(LogLevel.Debug);
        });

        var logger = loggersFactory.CreateLogger<SimulationDatabaseProcessor>();

        return new SimulationDatabaseProcessor(dbContext, logger);
    }

    private static Runs MapRunningInfoDtoToRuns(RunningInfoDto runningInfoDto)
    {
        return new Runs
        {
            Step = runningInfoDto.Step,
            Duration = runningInfoDto.Duration,
            SimulationState = runningInfoDto.SimulationState,
            Philosophers = [.. runningInfoDto.Philosophers.Select(p => MapPhilosophersDtoToPhilosophersEntity(p))]
        };
    }

    private static PhilosophersEntity MapPhilosophersDtoToPhilosophersEntity(PhilosophersDto philosophersDto)
    {
        return new PhilosophersEntity
        {
            PhilosopherState = philosophersDto.PhilosopherState,
            Forks = [MapForksDtoToForksEntity(philosophersDto.LeftFork), MapForksDtoToForksEntity(philosophersDto.RightFork)]
        };
    }

    private static ForksEntity MapForksDtoToForksEntity(ForksDto philosophersDto)
    {
        return new ForksEntity
        {
            ForkState = philosophersDto.ForkState
        };
    }

    private static RunningInfoDto MapRunsDtoToRunningInfo(Runs runningInfo)
    {
        return new RunningInfoDto(
            runningInfo.RunId,
            runningInfo.Step,
            runningInfo.Duration,
            runningInfo.SimulationState,
            [.. runningInfo.Philosophers.Select(p => MapPhilosophersEntityToPhilosophersDto(p))]);
    }

    private static PhilosophersDto MapPhilosophersEntityToPhilosophersDto(PhilosophersEntity philosophers)
    {
        if (philosophers.Forks.Count != 2)
        {
            throw new ApplicationException("Try map invalid philosopher object");
        }

        return new PhilosophersDto(
            philosophers.PhilosopherState,
            MapForksEntityToForksDto(philosophers.Forks[0]),
            MapForksEntityToForksDto(philosophers.Forks[1]));
    }

    private static ForksDto MapForksEntityToForksDto(ForksEntity philosophers)
    {
        return new ForksDto(philosophers.ForkState);
    }

    public async Task SaveRunningInfoAsync(RunningInfoDto runningInfoDto, CancellationToken ct)
    {
        var run = MapRunningInfoDtoToRuns(runningInfoDto);
        await _db.Runs.AddAsync(run, ct);
        if (await _db.SaveChangesAsync(ct) <= 0)
        {
            throw new ApplicationException("Entity not saved!");
        }

        _logger.LogInformation("Add new run with id: {0}", run.RunId);
    }

    public async Task<RunningInfoDto> GetRunningInfoByIdAsync(int id, CancellationToken ct)
    {
        var runningInfo = await _db.Runs
            .Include(r => r.Philosophers)
                .ThenInclude(p => p.Forks)
            .FirstAsync(x => x.RunId == id, ct);

        return MapRunsDtoToRunningInfo(runningInfo);
    }

    public async Task<IList<RunningInfoDto>> GetRunningInfoBySimulationStateAsync(SimulationStates state, CancellationToken ct)
    {
        var runningInfos = await _db.Runs
            .Where(x => x.SimulationState == state)
            .Include(r => r.Philosophers)
                .ThenInclude(p => p.Forks)
            .ToListAsync(ct);

        return [.. runningInfos.Select(p => MapRunsDtoToRunningInfo(p))];
    }

    public async Task<IList<RunningInfoDto>> GetRunningInfoByStepAsync(int step, CancellationToken ct)
    {
        var runningInfos = await _db.Runs
            .Where(x => x.Step == step)
            .Include(r => r.Philosophers)
                .ThenInclude(p => p.Forks)
            .ToListAsync(ct);

        return [.. runningInfos.Select(p => MapRunsDtoToRunningInfo(p))];
    }

    public async Task<IList<RunningInfoDto>> GetAllRunningInfosAsync(CancellationToken ct)
    {
        var runningInfos = await _db.Runs
            .Include(r => r.Philosophers)
                .ThenInclude(p => p.Forks)
            .ToListAsync(ct);

        return [.. runningInfos.Select(p => MapRunsDtoToRunningInfo(p))];
    }
}

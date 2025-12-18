using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Interface;
using Interface.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Philosophers.DB;
using Philosophers.DB.Entities;
using Philosophers.Services.DB.Service;

namespace Philosophers.Services.Tests;

public class SimulationDatabaseProcessorTests
{
    protected RunsContext _context;

    public SimulationDatabaseProcessorTests()
    {
        var options = new DbContextOptionsBuilder<RunsContext>()
                    .UseSqlite("Data Source=:memory:").Options;
        _context = new RunsContext(options);
    }

    private RunsContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RunsContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        var ctx = new RunsContext(options);
        ctx.Database.OpenConnection();      // важно для in-memory SQLite
        ctx.Database.EnsureCreated();
        return ctx;
    }

    private SimulationDatabaseProcessor CreateProcessor(RunsContext ctx)
    {
        var logger = Mock.Of<ILogger<SimulationDatabaseProcessor>>();
        return new SimulationDatabaseProcessor(ctx, logger);
    }

    private RunningInfoDto CreateTestDto(int step, SimulationStates state)
    {
        return new RunningInfoDto(
            Id: 0,
            Step: step,
            Duration: 5,
            SimulationState: state,
            Philosophers: new List<PhilosophersDto>
            {
                new PhilosophersDto(
                    PhilosopherState: "Thinking",
                    LeftFork: new ForksDto("Available"),
                    RightFork: new ForksDto("Available")
                )
            });
    }

    [Fact]
    public async Task SaveRunningInfoAsync_ShouldSaveRunWithRelations()
    {
        // Arrange
        using var ctx = CreateContext();
        var service = CreateProcessor(ctx);
        var dto = CreateTestDto(step: 10, state: SimulationStates.Running);

        // Act
        await service.SaveRunningInfoAsync(dto, CancellationToken.None);

        // Assert
        var run = ctx.Runs
            .Include(r => r.Philosophers)
                .ThenInclude(p => p.Forks)
            .FirstOrDefault();

        Assert.NotNull(run);
        Assert.Equal(10, run.Step);
        Assert.Single(run.Philosophers);
        Assert.Equal(2, run.Philosophers[0].Forks.Count);
    }

    [Fact]
    public async Task GetRunningInfoByIdAsync_ShouldReturnSavedEntity()
    {
        // Arrange
        using var ctx = CreateContext();
        var service = CreateProcessor(ctx);

        var dto = CreateTestDto(step: 3, state: SimulationStates.FinishSuccess);
        await service.SaveRunningInfoAsync(dto, CancellationToken.None);

        var id = ctx.Runs.First().RunId;

        // Act
        var result = await service.GetRunningInfoByIdAsync(id, CancellationToken.None);

        // Assert
        Assert.Equal(3, result.Step);
        Assert.Single(result.Philosophers);
    }

    [Fact]
    public async Task GetRunningInfoBySimulationStateAsync_ShouldReturnOnlyMatchingState()
    {
        // Arrange
        using var ctx = CreateContext();
        var service = CreateProcessor(ctx);

        await service.SaveRunningInfoAsync(CreateTestDto(1, SimulationStates.Running), CancellationToken.None);
        await service.SaveRunningInfoAsync(CreateTestDto(2, SimulationStates.FinishSuccess), CancellationToken.None);
        await service.SaveRunningInfoAsync(CreateTestDto(3, SimulationStates.Running), CancellationToken.None);

        // Act
        var result = await service.GetRunningInfoBySimulationStateAsync(
            SimulationStates.Running, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.Equal(SimulationStates.Running, r.SimulationState));
    }

    [Fact]
    public async Task GetRunningInfoByStepAsync_ShouldReturnEntitiesWithSameStep()
    {
        // Arrange
        using var ctx = CreateContext();
        var service = CreateProcessor(ctx);

        await service.SaveRunningInfoAsync(CreateTestDto(5, SimulationStates.Running), CancellationToken.None);
        await service.SaveRunningInfoAsync(CreateTestDto(5, SimulationStates.FinishSuccess), CancellationToken.None);
        await service.SaveRunningInfoAsync(CreateTestDto(7, SimulationStates.Running), CancellationToken.None);

        // Act
        var result = await service.GetRunningInfoByStepAsync(5, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.Equal(5, r.Step));
    }

    [Fact]
    public async Task GetAllRunningInfosAsync_ShouldReturnAllSavedRuns()
    {
        // Arrange
        using var ctx = CreateContext();
        var service = CreateProcessor(ctx);

        await service.SaveRunningInfoAsync(CreateTestDto(1, SimulationStates.Running), CancellationToken.None);
        await service.SaveRunningInfoAsync(CreateTestDto(2, SimulationStates.FinishSuccess), CancellationToken.None);
        await service.SaveRunningInfoAsync(CreateTestDto(3, SimulationStates.FinishSuccess), CancellationToken.None);

        // Act
        var result = await service.GetAllRunningInfosAsync(CancellationToken.None);

        // Assert
        Assert.Equal(3, result.Count);
    }
}

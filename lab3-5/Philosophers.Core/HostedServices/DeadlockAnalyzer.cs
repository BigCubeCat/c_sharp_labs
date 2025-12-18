using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Interface;
using Interface.Channel;
using Interface.DTO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Philosophers.Services.Channels.Items;

namespace Philosophers.Core.HostedServices;

public class DeadlockAnalyzer : BackgroundService
{
    private readonly IChannel<PhilosopherToAnalyzerChannelItem> _channel;
    private readonly ILogger<DeadlockAnalyzer> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private int _items = 0;

    public DeadlockAnalyzer(
        IChannel<PhilosopherToAnalyzerChannelItem> channel,
        IServiceScopeFactory scopeFactory,
        ILogger<DeadlockAnalyzer> logger)
    {
        _channel = channel;
        _logger = logger;
        _scopeFactory = scopeFactory;

        _channel.PublisherWantToRegister += PublisherWantToRegister;
    }

    private void PublisherWantToRegister(object? sender, EventArgs e)
    {
        ++_items;
    }

    private async Task SaveContextToDatabase(
        List<PhilosophersDto> philosophersDtos,
        ISimulationDatabaseProcessor databaseProcessor,
        CancellationToken stoppingToken)
    {
        await databaseProcessor.SaveRunningInfoAsync(
            new RunningInfoDto(0, -1,-1, SimulationStates.Deadlock, philosophersDtos),
            stoppingToken
        );
    }

    private async Task<List<PhilosopherToAnalyzerChannelItem>> ReadDataFromChannel(CancellationToken stoppingToken)
    {
        _channel.Notify(this);

        var dtos = new List<PhilosopherToAnalyzerChannelItem>(_items);
        for (int i = 0; i < _items; ++i)
        {
            var item = await _channel.Reader.ReadAsync(stoppingToken);
            dtos.Add(item);
        }

        return dtos;
    }

    private bool IsDeadlock(List<PhilosopherToAnalyzerChannelItem> philosophersDtos)
    {
        bool isAllForksUsed = true;

        for (int i = 0; i < _items; ++i)
        {
            var item = philosophersDtos[i];
            _logger.LogDebug("Philosopher {0}: {1}, {2}, {3}", i, item.LeftForkIsFree, item.RightForkIsFree, item.IAmEating);

            if (item.LeftForkIsFree || item.RightForkIsFree)
            {
                isAllForksUsed = false;
            }

            if (item.IAmEating)
            {
                return false;
            }
        }

        return isAllForksUsed;
    }

    public async Task<List<PhilosopherToAnalyzerChannelItem>?> Analyze(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var data = await ReadDataFromChannel(stoppingToken);

            if (IsDeadlock(data))
            {
                _logger.LogCritical("DEADLOCK DETECTED!");
                return data;
            }

            data.Clear();
            await Task.Delay(1000, stoppingToken);
        }

        return null;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var data = await Analyze(stoppingToken);
            if (data != null)
            {
                var dtos = new List<PhilosophersDto>(_items);
                for (int i = 0; i < _items; ++i)
                {
                    dtos.Add(
                        new PhilosophersDto
                        (
                            "Am I eating?: " + data[i].IAmEating,
                            new ForksDto("Is left fork free?: " + data[i].LeftForkIsFree),
                            new ForksDto("Is right fork free?: " + data[i].RightForkIsFree)
                        )
                    );
                }

                using var scope = _scopeFactory.CreateScope();
                var serviceProvider = scope.ServiceProvider;
                var dbContext = serviceProvider.GetRequiredService<ISimulationDatabaseProcessor>();

                await SaveContextToDatabase(dtos, dbContext, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Deadlock analyzer shutdown!");
        }

        return;
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interface;
using Interface.Channel;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Philosophers.Services.Channels.Items;
using Philosophers.Services.Channels.Events;
using Microsoft.Extensions.DependencyInjection;
using Interface.DTO;

namespace Philosophers.Core.HostedServices;

public class SimulationManager : BackgroundService, ISimulationManager
{
    private readonly IChannel<PhilosopherToPrinterChannelItem> _channel;
    private readonly ILogger<SimulationManager> _logger;
    private readonly CompletionCoordinator _coordinator;
    private readonly IServiceScopeFactory _scopeFactory;
    private int _step = 0;
    private readonly Stopwatch _stopwatch = new();
    private int _items = 0;
    private readonly int _steps;

    public SimulationManager(
        IOptions<PhilosopherConfiguration> options,
        IChannel<PhilosopherToPrinterChannelItem> channel,
        ILogger<SimulationManager> logger,
        IServiceScopeFactory scopeFactory,
        CompletionCoordinator coordinator)
    {
        _channel = channel;
        _logger = logger;
        _coordinator = coordinator;
        _steps = options.Value.Steps;
        _scopeFactory = scopeFactory;

        _channel.PublisherWantToRegister += PublisherWantToRegister;
    }

    private void PublisherWantToRegister(object? sender, EventArgs e)
    {
        ++_items;
    }

    private async Task<List<PhilosophersDto>> ReadDataFromChannel(CancellationToken stoppingToken)
    {
        _channel.Notify(this);

        var dtos = new List<PhilosophersDto>(_items);
        for (int i = 0; i < _items; ++i)
        {
            var item = await _channel.Reader.ReadAsync(stoppingToken);
            dtos.Add(
                new PhilosophersDto
                (
                    item.PhilosopherInfo,
                    new ForksDto(item.LeftForkInfo),
                    new ForksDto(item.RightForkInfo)
                )
            );
        }

        return dtos;
    }

    private async Task<List<PhilosophersDto>> ReadDataFromChannel(
        IChannelEventArgs args,
        CancellationToken stoppingToken)
    {
        _channel.NotifyWith(this, args);

        var dtos = new List<PhilosophersDto>(_items);
        for (int i = 0; i < _items; ++i)
        {
            var item = await _channel.Reader.ReadAsync(stoppingToken);
            dtos.Add(
                new PhilosophersDto
                (
                    item.PhilosopherInfo,
                    new ForksDto(item.LeftForkInfo),
                    new ForksDto(item.RightForkInfo)
                )
            );
        }

        return dtos;
    }

    private void PrintInfo(List<PhilosophersDto> philosophersDtos)
    {
        _channel.Notify(this);

        Console.Clear();
        Console.WriteLine("==============STEP{0}==============", _step);

        for (int i = 0; i < _items; ++i)
        {
            var item = philosophersDtos[i];
            Console.WriteLine(item.PhilosopherState);
            Console.WriteLine(" |- Left Fork: " + item.LeftFork.ForkState);
            Console.WriteLine(" |- Right Fork: " + item.RightFork.ForkState);
        }
    }

    private async Task SaveContextToDatabase(
        ISimulationDatabaseProcessor databaseProcessor,
        List<PhilosophersDto> philosophersDtos,
        SimulationStates state,
        CancellationToken stoppingToken)
    {
        await databaseProcessor.SaveRunningInfoAsync(
            new RunningInfoDto(0, _step, _stopwatch.ElapsedMilliseconds,
                state, philosophersDtos),
            stoppingToken
        );
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        List<PhilosophersDto> data;
        IServiceScope scope;
        IServiceProvider serviceProvider;
        ISimulationDatabaseProcessor dbContext;

        SimulationStates state = SimulationStates.FinishSuccess;
        bool isException = false;

        try
        {
            _coordinator.RegisterService(GetType().Name);

            _stopwatch.Start();
            while (!stoppingToken.IsCancellationRequested && _step < _steps)
            {
                ++_step;

                scope = _scopeFactory.CreateScope();
                serviceProvider = scope.ServiceProvider;

                dbContext = serviceProvider.GetRequiredService<ISimulationDatabaseProcessor>();
                data = await ReadDataFromChannel(stoppingToken);
                await SaveContextToDatabase(dbContext, data, SimulationStates.Running, stoppingToken);
                PrintInfo(data);

                scope.Dispose();
                await Task.Delay(1000, stoppingToken);
            }
            state = SimulationStates.FinishSuccess;
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Simulation manager shutdown");

            state = SimulationStates.FinishSuccess;
        }
        catch (Exception e)
        {
            _logger.LogCritical($"Unexpected exception from {e.Source}.\nMessage: {e.Message}.\nStack Trace: {e.StackTrace}.");

            state = SimulationStates.FinishError;
            isException = true;
        }
        finally
        {
            _stopwatch.Stop();

            scope = _scopeFactory.CreateScope();
            serviceProvider = scope.ServiceProvider;

            dbContext = serviceProvider.GetRequiredService<ISimulationDatabaseProcessor>();

            if (isException)
            {
                data = await ReadDataFromChannel(stoppingToken);
            }
            else
            {
                data = await ReadDataFromChannel(
                    new ChannelScoresEvent() { SimulationTime = _stopwatch.ElapsedMilliseconds },
                    stoppingToken);
            }

            await SaveContextToDatabase(dbContext, data, state, stoppingToken);
            PrintInfo(data);

            scope.Dispose();

            _coordinator.CompleteService(GetType().Name);
        }

        return;
    }
}

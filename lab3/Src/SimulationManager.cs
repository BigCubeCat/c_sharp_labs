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
using Src.Channels.Events;
using Src.Channels.Items;

namespace Src;

public class SimulationManager : BackgroundService, ISimulationManager
{
    private readonly IChannel<PhilosopherToPrinterChannelItem> _channel;
    private readonly ILogger<SimulationManager> _logger;
    private readonly CompletionCoordinator _coordinator;
    private int _step = 0;
    private readonly Stopwatch _stopwatch = new();
    private int _items = 0;
    private readonly int _steps;

    public SimulationManager(
        IOptions<PhilosopherConfiguration> options,
        IChannel<PhilosopherToPrinterChannelItem> channel,
        ILogger<SimulationManager> logger,
        CompletionCoordinator coordinator)
    {
        _channel = channel;
        _logger = logger;
        _coordinator = coordinator;
        _steps = options.Value.Steps;

        _channel.PublisherWantToRegister += PublisherWantToRegister;
    }

    private void PublisherWantToRegister(object? sender, EventArgs e)
    {
        ++_items;
    }

    private async Task PrintInfo(CancellationToken stoppingToken)
    {
        _channel.Notify(this);
        PhilosopherToPrinterChannelItem item;

        ++_step;
        Console.Clear();
        Console.WriteLine("==============STEP{0}==============", _step);

        for (int i = 0; i < _items; ++i)
        {
            item = await _channel.Reader.ReadAsync(stoppingToken);
            Console.WriteLine(item.PhilosopherInfo);
            Console.WriteLine(" |- Left Fork: " + item.LeftForkInfo);
            Console.WriteLine(" |- Right Fork: " + item.RightForkInfo);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _coordinator.RegisterService(GetType().Name);

            _stopwatch.Start();
            while (!stoppingToken.IsCancellationRequested && _step < _steps)
            {
                await PrintInfo(stoppingToken);
                Thread.Sleep(1000);
            }
            _stopwatch.Stop();

            _channel.NotifyWith(this, new ChannelScoresEvent(){SimulationTime = _stopwatch.ElapsedMilliseconds});
            
            PhilosopherToPrinterChannelItem item;
            Console.WriteLine("FinalStats");
            for (int i = 0; i < _items; ++i)
            {
                item = await _channel.Reader.ReadAsync(stoppingToken);
                Console.WriteLine(item.PhilosopherInfo);
                Console.WriteLine(" |- Left Fork: " + item.LeftForkInfo);
                Console.WriteLine(" |- Right Fork: " + item.RightForkInfo);
            }

            _coordinator.CompleteService(GetType().Name);
        }
        catch (OperationCanceledException) {
            Console.WriteLine("Application shutdown forced, can not print stats");
        }

        return;
    }
}

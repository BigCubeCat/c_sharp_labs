using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Interface.Channel;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Lab.Core.Channels.Items;

namespace Lab.Core;

public class DeadlockAnalyzer : BackgroundService
{
    private readonly IChannel<PhilosopherToAnalyzerChannelItem> _channel;
    private readonly ILogger<DeadlockAnalyzer> _logger;
    private int _items = 0;

    public DeadlockAnalyzer(
        IChannel<PhilosopherToAnalyzerChannelItem> channel,
        ILogger<DeadlockAnalyzer> logger)
    {
        _channel = channel;
        _logger = logger;

        _channel.PublisherWantToRegister += PublisherWantToRegister;
    }

    private void PublisherWantToRegister(object? sender, EventArgs e)
    {
        ++_items;
    }

    private async Task<bool> IsDeadlock(CancellationToken stoppingToken)
    {
        _channel.Notify(this);

        bool isAllForksUsed = true;

        PhilosopherToAnalyzerChannelItem item;

        for (int i = 0; i < _items; ++i)
        {
            item = await _channel.Reader.ReadAsync(stoppingToken);
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

    public async Task Analyze(CancellationToken stoppingToken)
    {
        while (!await IsDeadlock(stoppingToken))
        {
            if (!stoppingToken.IsCancellationRequested)
            {
                return;
            }
            Thread.Sleep(1000);
        }

        Console.WriteLine("DEADLOCK DETECTED!");
        _logger.LogCritical("DEADLOCK DETECTED!");
        throw new ApplicationException("Deadlock");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await Analyze(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            throw new ApplicationException("Operation cancelled not normally");
        }

        return;
    }
}

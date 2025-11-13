using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Lab.Core;
using Lab.Core.Channels.Items;
using Interface.Channel;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Lab.Tests;

public class DeadlockTest
{
    private class FakeChannel : IChannel<PhilosopherToAnalyzerChannelItem>
    {
        public event EventHandler? SendMeItem;
        public event EventHandler<IChannelEventArgs>? SendMeItemBy;
        public event EventHandler? PublisherWantToRegister;

        private readonly Channel<PhilosopherToAnalyzerChannelItem> _channel;
        private int _itemCount = 0;

        public ChannelWriter<PhilosopherToAnalyzerChannelItem> Writer => _channel.Writer;
        public ChannelReader<PhilosopherToAnalyzerChannelItem> Reader => _channel.Reader;

        public FakeChannel()
        {
            _channel = Channel.CreateUnbounded<PhilosopherToAnalyzerChannelItem>();
        }

        public void Notify(object? sender)
        {
            // When notified, trigger SendMeItem to simulate philosophers sending data
            SendMeItem?.Invoke(sender, EventArgs.Empty);
        }

        public void NotifyWith(object? sender, IChannelEventArgs args)
        {
            SendMeItemBy?.Invoke(sender, args);
        }

        public void RegisterPublisher(object? publisher)
        {
            _itemCount++;
            PublisherWantToRegister?.Invoke(publisher, EventArgs.Empty);
        }

        // Helper method to simulate philosophers writing to the channel
        public void SimulatePhilosopherData(PhilosopherToAnalyzerChannelItem item)
        {
            _channel.Writer.TryWrite(item);
        }

        public void SetItemCount(int count)
        {
            _itemCount = count;
        }
    }

    private DeadlockAnalyzer CreateAnalyzer(FakeChannel ch)
    {
        var logger = new Mock<ILogger<DeadlockAnalyzer>>();
        return new DeadlockAnalyzer(ch, logger.Object);
    }

    // ----------------------------------------------------------
    // 1. DEADLOCK: all philosophers hold forks but none are eating
    // ----------------------------------------------------------
    [Fact]
    public async Task DetectsDeadlock_WhenAllPhilosophersHoldRightForkOnly()
    {
        var channel = new FakeChannel();
        var analyzer = CreateAnalyzer(channel);

        // Register 5 philosophers
        for (int i = 0; i < 5; i++)
        {
            channel.RegisterPublisher(this);
        }

        // Set up the event handler to provide data when requested
        channel.SendMeItem += (sender, e) =>
        {
            // Simulate 5 philosophers reporting deadlock conditions
            for (int i = 0; i < 5; i++)
            {
                channel.SimulatePhilosopherData(
                    new PhilosopherToAnalyzerChannelItem(
                        IAmEating: false,
                        LeftForkIsFree: false,
                        RightForkIsFree: false
                    )
                );
            }
        };

        var token = new CancellationTokenSource(1000).Token; // 1 second timeout

        await Assert.ThrowsAsync<ApplicationException>(async () =>
        {
            await analyzer.Analyze(token);
        });
    }

    // ----------------------------------------------------------
    // 2. NO DEADLOCK: at least one philosopher is eating
    // ----------------------------------------------------------
    [Fact]
    public async Task NoDeadlock_WhenOnePhilosopherIsEating()
    {
        var channel = new FakeChannel();
        var analyzer = CreateAnalyzer(channel);

        // Register 5 philosophers
        for (int i = 0; i < 5; i++)
        {
            channel.RegisterPublisher(this);
        }

        // Set up the event handler to provide data when requested
        channel.SendMeItem += (sender, e) =>
        {
            // First philosopher is eating
            channel.SimulatePhilosopherData(
                new PhilosopherToAnalyzerChannelItem(
                    IAmEating: true,  // This philosopher is eating
                    LeftForkIsFree: false,
                    RightForkIsFree: false
                )
            );

            // Remaining philosophers are not eating but forks might be available
            for (int i = 1; i < 5; i++)
            {
                channel.SimulatePhilosopherData(
                    new PhilosopherToAnalyzerChannelItem(
                        IAmEating: false,
                        LeftForkIsFree: (i % 2 == 0), // Some forks are free
                        RightForkIsFree: (i % 2 == 1) // Some forks are free
                    )
                );
            }
        };

        var token = new CancellationTokenSource(1000).Token; // 1 second timeout

        // This should complete without throwing (no deadlock detected)
        await analyzer.Analyze(token);

        // If we reach here, the test passes (no deadlock exception thrown)
    }
}

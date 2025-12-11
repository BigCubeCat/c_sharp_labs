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

    [Fact]
    public async Task DetectsDeadlock_WhenAllPhilosophersHoldRightForkOnly()
    {
        // Arrange
        var channel = new FakeChannel();
        var analyzer = CreateAnalyzer(channel);

        // Register 5 philosophers
        for (int i = 0; i < 5; i++)
        {
            channel.RegisterPublisher(this);
        }

        // When analyzer notifies, supply 5 deadlock-like items
        channel.SendMeItem += (sender, e) =>
        {
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

        // Act & Assert
        await Assert.ThrowsAsync<ApplicationException>(async () =>
        {
            await analyzer.Analyze(token);
        });
    }

    [Fact]
    public async Task NoDeadlock_WhenOnePhilosopherIsEating()
    {
        // Arrange
        var channel = new FakeChannel();
        var analyzer = CreateAnalyzer(channel);

        // Register 5 philosophers
        for (int i = 0; i < 5; i++)
        {
            channel.RegisterPublisher(this);
        }

        // Provide data: first philosopher is eating, others may have free forks
        channel.SendMeItem += (sender, e) =>
        {
            // First philosopher is eating
            channel.SimulatePhilosopherData(
                new PhilosopherToAnalyzerChannelItem(
                    IAmEating: true,
                    LeftForkIsFree: false,
                    RightForkIsFree: false
                )
            );

            // Remaining philosophers
            for (int i = 1; i < 5; i++)
            {
                channel.SimulatePhilosopherData(
                    new PhilosopherToAnalyzerChannelItem(
                        IAmEating: false,
                        LeftForkIsFree: (i % 2 == 0),
                        RightForkIsFree: (i % 2 == 1)
                    )
                );
            }
        };

        var token = new CancellationTokenSource(1000).Token; // 1 second timeout

        // Act
        await analyzer.Analyze(token);

        // Assert: no exception thrown => success (reaching this point means pass)
    }
}

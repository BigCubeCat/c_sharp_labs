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
        public event EventHandler? PublisherWantToRegister;
        public Channel<PhilosopherToAnalyzerChannelItem> Inner = Channel.CreateUnbounded<PhilosopherToAnalyzerChannelItem>();

        public ChannelReader<PhilosopherToAnalyzerChannelItem> Reader => Inner.Reader;

        public void Notify(object sender)
        {
            // имитация запроса количества философов
        }

        public void Publish(PhilosopherToAnalyzerChannelItem item)
        {
            PublisherWantToRegister?.Invoke(this, EventArgs.Empty);
            Inner.Writer.TryWrite(item);
        }
    }

    private DeadlockAnalyzer CreateAnalyzer(FakeChannel ch)
    {
        var logger = new Mock<ILogger<DeadlockAnalyzer>>();
        return new DeadlockAnalyzer(ch, logger.Object);
    }

    // ----------------------------------------------------------
    // 1. DEADLOCK: все держат правую вилку, никто не ест
    // ----------------------------------------------------------
    [Fact]
    public async Task DetectsDeadlock_WhenAllPhilosophersHoldRightForkOnly()
    {
        var channel = new FakeChannel();
        var analyzer = CreateAnalyzer(channel);
        var token = new CancellationTokenSource().Token;

        // 5 философов — все НЕ едят, левая занята, правая занята
        for (int i = 0; i < 5; i++)
        {
            channel.Publish(
                new PhilosopherToAnalyzerChannelItem(
                    IAmEating: false,
                    LeftForkIsFree: false,
                    RightForkIsFree: false
                )
            );
        }

        await Assert.ThrowsAsync<ApplicationException>(async () =>
        {
            await analyzer.Analyze(token);
        });
    }

    // ----------------------------------------------------------
    // 2. NO DEADLOCK: хоть один философ ест
    // ----------------------------------------------------------
    [Fact]
    public async Task NoDeadlock_WhenOnePhilosopherIsEating()
    {
        var channel = new FakeChannel();
        var analyzer = CreateAnalyzer(channel);
        var cts = new CancellationTokenSource();
        cts.CancelAfter(200); // чтобы цикл analyzer.Analyze не завис
        var token = cts.Token;

        // Философ 0 ест → дедлока нет
        channel.Publish(
            new PhilosopherToAnalyzerChannelItem(
                IAmEating: true,
                LeftForkIsFree: false,
                RightForkIsFree: false
            )
        );

        // Остальные просто ждут
        for (int i = 1; i < 5; i++)
        {
            channel.Publish(
                new PhilosopherToAnalyzerChannelItem(
                    IAmEating: false,
                    LeftForkIsFree: false,
                    RightForkIsFree: false
                )
            );
        }

        // Метод Analyze должен завершиться БЕЗ исключения
        await analyzer.Analyze(token);
    }
}

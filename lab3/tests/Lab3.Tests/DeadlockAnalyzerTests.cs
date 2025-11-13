using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Interface; // DeadlockAnalyzer находится в namespace Interface
using Src.Channels.Items; // Channel items находятся в Src.Channels.Items
using Xunit;
using Lab3.Tests.TestHelpers;

namespace Lab3.Tests
{
    public class DeadlockAnalyzerTests
    {
        [Fact]
        public async Task DeadlockAnalyzer_Throws_OnDeadlock()
        {
            var channel = new TestChannel<PhilosopherToAnalyzerChannelItem>();
            var logger = new Mock<ILogger<DeadlockAnalyzer>>().Object;
            var analyzer = new DeadlockAnalyzer(channel, logger);

            // Регистрируем 3 издателя => analyzer ожидает 3 элемента
            channel.RegisterPublisher(this);
            channel.RegisterPublisher(this);
            channel.RegisterPublisher(this);

            // Записываем 3 элемента: никто не ест, обе вилки заняты (LeftForkIsFree=false, RightForkIsFree=false)
            await channel.Writer.WriteAsync(new PhilosopherToAnalyzerChannelItem(false, false, false));
            await channel.Writer.WriteAsync(new PhilosopherToAnalyzerChannelItem(false, false, false));
            await channel.Writer.WriteAsync(new PhilosopherToAnalyzerChannelItem(false, false, false));

            var cts = new CancellationTokenSource();
            // Ожидаем исключения (Deadlock)
            await Assert.ThrowsAsync<ApplicationException>(() => analyzer.Analyze(cts.Token));
        }

        [Fact]
        public async Task DeadlockAnalyzer_Returns_WhenNotDeadlocked()
        {
            var channel = new TestChannel<PhilosopherToAnalyzerChannelItem>();
            var logger = new Mock<ILogger<DeadlockAnalyzer>>().Object;
            var analyzer = new DeadlockAnalyzer(channel, logger);

            channel.RegisterPublisher(this);
            channel.RegisterPublisher(this);

            // Один из философов ест -> не deadlock
            await channel.Writer.WriteAsync(new PhilosopherToAnalyzerChannelItem(true, false, false));
            await channel.Writer.WriteAsync(new PhilosopherToAnalyzerChannelItem(false, false, false));

            var cts = new CancellationTokenSource();
            // Не должно бросать, а функция должна вернуть (Analyze сделает early return)
            await analyzer.Analyze(cts.Token);
        }
    }
}

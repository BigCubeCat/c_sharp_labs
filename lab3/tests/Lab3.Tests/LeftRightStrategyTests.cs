using System.Threading;
using Interface.Channel;
using Microsoft.Extensions.Options;
using Moq;
using Src.Channels.Items;
using Src.Philosophers;
using Src.Strategy;
using Xunit;
using Lab3.Tests.TestHelpers;
using Microsoft.Extensions.Logging;
using Interface.Strategy; // <- добавлено

namespace Lab3.Tests
{
    public class LeftRightStrategyTests
    {
        PhilosopherA CreatePhilosopherA(IStrategy strategy)
        {
            var logger = new Mock<ILogger<PhilosopherService>>().Object;
            var opts = Options.Create(new Interface.PhilosopherConfiguration { EatingTimeMin = 1, EatingTimeMax = 2, TakeForkTimeMin = 1, TakeForkTimeMax = 2, ThinkingTimeMin = 1, ThinkingTimeMax = 2, Steps = 1 });
            var factory = new TestForkFactory();
            var channelAnalyzer = new TestChannel<PhilosopherToAnalyzerChannelItem>();
            var channelPrinter = new TestChannel<PhilosopherToPrinterChannelItem>();
            return new PhilosopherA(logger, strategy, opts, factory, channelAnalyzer, channelPrinter);
        }

        PhilosopherB CreatePhilosopherB(IStrategy strategy)
        {
            var logger = new Mock<ILogger<PhilosopherService>>().Object;
            var opts = Options.Create(new Interface.PhilosopherConfiguration { EatingTimeMin = 1, EatingTimeMax = 2, TakeForkTimeMin = 1, TakeForkTimeMax = 2, ThinkingTimeMin = 1, ThinkingTimeMax = 2, Steps = 1 });
            var factory = new TestForkFactory();
            var channelAnalyzer = new TestChannel<PhilosopherToAnalyzerChannelItem>();
            var channelPrinter = new TestChannel<PhilosopherToPrinterChannelItem>();
            return new PhilosopherB(logger, strategy, opts, factory, channelAnalyzer, channelPrinter);
        }

        [Fact]
        public void LeftRightStrategy_RightHanded_TakesRightFirst()
        {
            var strategy = new LeftRightStrategy();
            var philosopher = CreatePhilosopherA(strategy);

            Assert.False(philosopher.LeftFork.IsLockedBy(philosopher));
            Assert.False(philosopher.RightFork.IsLockedBy(philosopher));

            strategy.LockFork(philosopher);

            Assert.True(philosopher.RightFork.IsLockedBy(philosopher) || !philosopher.LeftFork.IsLockedBy(philosopher));
        }

        [Fact]
        public void LeftRightStrategy_LeftHanded_TakesLeftFirst()
        {
            var strategy = new LeftRightStrategy();
            var philosopher = CreatePhilosopherB(strategy);

            strategy.LockFork(philosopher);
            Assert.True(philosopher.LeftFork.IsLockedBy(philosopher) || !philosopher.RightFork.IsLockedBy(philosopher));
        }
    }
}

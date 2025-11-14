using System;
using System.Threading;
using System.Threading.Tasks;
using Interface;
using Interface.Channel;
using Interface.Strategy;
using Lab.Core;
using Lab.Core.Channels;
using Lab.Core.Channels.Items;
using Lab.Core.Philosophers;
using Lab.Core.Strategy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Lab.Tests;

public class PhilosopherStateTransitionsTest
{
    private PhilosopherService CreatePhilosopher<T>(
        IStrategy strategy,
        PhilosopherConfiguration config,
        IFork leftFork,
        IFork rightFork) where T : PhilosopherService
    {
        var loggerMock = new Mock<ILogger<PhilosopherService>>();
        var optionsMock = new Mock<IOptions<PhilosopherConfiguration>>();
        optionsMock.Setup(o => o.Value).Returns(config);

        var forksFactoryMock = new Mock<IForksFactory<Fork>>();
        forksFactoryMock.SetupSequence(f => f.Create())
            .Returns(leftFork)
            .Returns(rightFork);

        var channelToAnalyzerMock = new Mock<IChannel<PhilosopherToAnalyzerChannelItem>>();
        var channelToPrinterMock = new Mock<IChannel<PhilosopherToPrinterChannelItem>>();

        if (typeof(T) == typeof(PhilosopherA)) // Левша
        {
            return new PhilosopherA(
                loggerMock.Object,
                strategy,
                optionsMock.Object,
                forksFactoryMock.Object,
                channelToAnalyzerMock.Object,
                channelToPrinterMock.Object
            );
        }
        else
        {
            return new PhilosopherB(
                loggerMock.Object,
                strategy,
                optionsMock.Object,
                forksFactoryMock.Object,
                channelToAnalyzerMock.Object,
                channelToPrinterMock.Object
            );
        }
    }

    private Mock<IFork> CreateAvailableFork()
    {
        var forkMock = new Mock<IFork>();
        forkMock.Setup(f => f.TryTake(It.IsAny<IPhilosopher>()));
        forkMock.Setup(f => f.TryLock(It.IsAny<IPhilosopher>()));
        forkMock.Setup(f => f.IsLockedBy(It.IsAny<IPhilosopher>())).Returns(true);
        forkMock.Setup(f => f.IsTakenBy(It.IsAny<IPhilosopher>())).Returns(true);
        forkMock.Setup(f => f.UnlockFork());
        forkMock.Setup(f => f.Put());
        forkMock.Setup(f => f.GetInfoString()).Returns("Mock Fork - Available");
        forkMock.Setup(f => f.GetScoreString(It.IsAny<double>())).Returns("Mock Fork - Score");

        return forkMock;
    }

    private Mock<IFork> CreateUnavailableFork()
    {
        var forkMock = new Mock<IFork>();
        forkMock.Setup(f => f.TryTake(It.IsAny<IPhilosopher>()));
        forkMock.Setup(f => f.TryLock(It.IsAny<IPhilosopher>()));
        forkMock.Setup(f => f.IsLockedBy(It.IsAny<IPhilosopher>())).Returns(false);
        forkMock.Setup(f => f.IsTakenBy(It.IsAny<IPhilosopher>())).Returns(false);
        forkMock.Setup(f => f.UnlockFork());
        forkMock.Setup(f => f.Put());
        forkMock.Setup(f => f.GetInfoString()).Returns("Mock Fork - Unavailable");
        forkMock.Setup(f => f.GetScoreString(It.IsAny<double>())).Returns("Mock Fork - Score");

        return forkMock;
    }

    [Fact]
    public void Philosopher_InitialState_Correct()
    {
        // Arrange
        var config = new PhilosopherConfiguration();
        var leftForkMock = CreateAvailableFork();
        var rightForkMock = CreateAvailableFork();
        var strategy = new LeftRightStrategy();

        var philosopher = CreatePhilosopher<PhilosopherB>(strategy, config, leftForkMock.Object, rightForkMock.Object);

        // Act & Assert
        Assert.NotNull(philosopher.Name);
        Assert.Equal("B", philosopher.Name);
        Assert.NotNull(philosopher.LeftFork);
        Assert.NotNull(philosopher.RightFork);
        Assert.Equal(0, philosopher.CountEatingFood);
        Assert.Equal(0, philosopher.HungryTime);

        var info = philosopher.GetInfoString();
        Assert.Contains(philosopher.Name, info);
    }

    [Fact]
    public void Philosopher_ImplementsInterfaces_Correctly()
    {
        // Arrange
        var config = new PhilosopherConfiguration();
        var leftForkMock = CreateAvailableFork();
        var rightForkMock = CreateAvailableFork();
        var strategy = new LeftRightStrategy();

        var philosopher = CreatePhilosopher<PhilosopherB>(strategy, config, leftForkMock.Object, rightForkMock.Object);

        // Act & Assert
        Assert.IsAssignableFrom<IPhilosopher>(philosopher);
        Assert.IsAssignableFrom<IAccessible>(philosopher);
    }

    [Fact]
    public void Fork_ImplementsInterfaces_Correctly()
    {
        // Arrange
        var fork = new Fork(1);

        // Act & Assert
        Assert.IsAssignableFrom<IFork>(fork);
        Assert.IsAssignableFrom<IAccessible>(fork);

        var info = fork.GetInfoString();
        var score = fork.GetScoreString(1000);

        Assert.NotNull(info);
        Assert.NotNull(score);
        Assert.Contains("Fork-1", info);
    }

    [Fact]
    public void Strategy_TakeFork_ForRightHanded_WorksCorrectly()
    {
        // Arrange
        var strategy = new LeftRightStrategy();
        var leftForkMock = new Mock<IFork>();
        var rightForkMock = new Mock<IFork>();

        var config = new PhilosopherConfiguration();
        var philosopher = CreatePhilosopher<PhilosopherB>(strategy, config, leftForkMock.Object, rightForkMock.Object);

        // Act - For right-handed philosopher (PhilosopherB)
        strategy.TakeFork(philosopher);

        // Assert - Should try to take right fork for right-handed
        rightForkMock.Verify(f => f.TryTake(philosopher), Times.Once);
        leftForkMock.Verify(f => f.TryTake(philosopher), Times.Never);
    }

    [Fact]
    public void Strategy_TakeFork_ForLeftHanded_WorksCorrectly()
    {
        // Arrange
        var strategy = new LeftRightStrategy();
        var leftForkMock = new Mock<IFork>();
        var rightForkMock = new Mock<IFork>();

        var config = new PhilosopherConfiguration();
        var philosopher = CreatePhilosopher<PhilosopherA>(strategy, config, leftForkMock.Object, rightForkMock.Object);

        // Act - For left-handed philosopher (PhilosopherB)
        strategy.TakeFork(philosopher);

        // Assert - Should try to take left fork for left-handed
        leftForkMock.Verify(f => f.TryTake(philosopher), Times.Once);
        rightForkMock.Verify(f => f.TryTake(philosopher), Times.Never);
    }

    [Fact]
    public void Strategy_LockFork_ForRightHanded_WorksCorrectly()
    {
        // Arrange
        var strategy = new LeftRightStrategy();
        var leftForkMock = new Mock<IFork>();
        var rightForkMock = new Mock<IFork>();

        var config = new PhilosopherConfiguration();
        var philosopher = CreatePhilosopher<PhilosopherB>(strategy, config, leftForkMock.Object, rightForkMock.Object);

        // Act - For right-handed philosopher (PhilosopherB)
        strategy.LockFork(philosopher);

        // Assert - Should try to lock right fork for right-handed
        rightForkMock.Verify(f => f.TryLock(philosopher), Times.Once);
        leftForkMock.Verify(f => f.TryLock(philosopher), Times.Never);
    }

    [Fact]
    public void Strategy_LockFork_ForLeftHanded_WorksCorrectly()
    {
        // Arrange
        var strategy = new LeftRightStrategy();
        var leftForkMock = new Mock<IFork>();
        var rightForkMock = new Mock<IFork>();

        var config = new PhilosopherConfiguration();
        var philosopher = CreatePhilosopher<PhilosopherA>(strategy, config, leftForkMock.Object, rightForkMock.Object);

        // Act - For left-handed philosopher (PhilosopherA)
        strategy.LockFork(philosopher);

        // Assert - Should try to lock left fork for left-handed
        leftForkMock.Verify(f => f.TryLock(philosopher), Times.Once);
        rightForkMock.Verify(f => f.TryLock(philosopher), Times.Never);
    }

    [Fact]
    public void Strategy_PutForks_WorksCorrectly()
    {
        // Arrange
        var strategy = new LeftRightStrategy();
        var leftForkMock = new Mock<IFork>();
        var rightForkMock = new Mock<IFork>();

        var config = new PhilosopherConfiguration();
        var philosopher = CreatePhilosopher<PhilosopherB>(strategy, config, leftForkMock.Object, rightForkMock.Object);

        // Setup that philosopher has both forks
        leftForkMock.Setup(f => f.IsTakenBy(philosopher)).Returns(true);
        rightForkMock.Setup(f => f.IsTakenBy(philosopher)).Returns(true);

        // Act
        strategy.PutForks(philosopher);

        // Assert - Should put both forks
        leftForkMock.Verify(f => f.Put(), Times.Once);
        rightForkMock.Verify(f => f.Put(), Times.Once);
    }

    [Fact]
    public void Strategy_UnlockForks_WorksCorrectly()
    {
        // Arrange
        var strategy = new LeftRightStrategy();
        var leftForkMock = new Mock<IFork>();
        var rightForkMock = new Mock<IFork>();

        var config = new PhilosopherConfiguration();
        var philosopher = CreatePhilosopher<PhilosopherB>(strategy, config, leftForkMock.Object, rightForkMock.Object);

        // Setup that philosopher has locked both forks
        leftForkMock.Setup(f => f.IsLockedBy(philosopher)).Returns(true);
        rightForkMock.Setup(f => f.IsLockedBy(philosopher)).Returns(true);

        // Act
        strategy.UnlockForks(philosopher);

        // Assert - Should unlock both forks
        leftForkMock.Verify(f => f.UnlockFork(), Times.Once);
        rightForkMock.Verify(f => f.UnlockFork(), Times.Once);
    }

    [Fact]
    public void Fork_GetScoreString_CalculatesCorrectPercentages()
    {
        // Arrange
        var fork = new Fork(1);

        // Simulate some usage by taking and putting the fork
        var philosopherMock = new Mock<IPhilosopher>();
        fork.TryLock(philosopherMock.Object);
        Thread.Sleep(10); // Small delay to get some time
        fork.TryTake(philosopherMock.Object);
        Thread.Sleep(10);
        fork.Put();

        // Act
        var score = fork.GetScoreString(100.0); // 100ms simulation time

        // Assert
        Assert.NotNull(score);
        Assert.Contains("Fork-1", score);
        Assert.Contains("used", score);
        Assert.Contains("available", score);
        Assert.Contains("blocked", score);
    }

    [Fact]
    public void Philosopher_GetScoreString_CalculatesCorrectStatistics()
    {
        // Arrange
        var config = new PhilosopherConfiguration();
        var leftForkMock = CreateAvailableFork();
        var rightForkMock = CreateAvailableFork();
        var strategy = new LeftRightStrategy();

        var philosopher = CreatePhilosopher<PhilosopherB>(strategy, config, leftForkMock.Object, rightForkMock.Object);

        // Act
        var score = philosopher.GetScoreString(1000.0); // 1000ms simulation time

        // Assert
        Assert.NotNull(score);
        Assert.Contains(philosopher.Name, score);
        Assert.Contains("throughput", score);
        Assert.Contains("hungry", score);
    }

    [Fact]
    public void Strategy_HasLeftFork_ReturnsCorrectValue()
    {
        // Arrange
        var strategy = new LeftRightStrategy();
        var leftForkMock = new Mock<IFork>();
        var rightForkMock = new Mock<IFork>();

        var config = new PhilosopherConfiguration();
        var philosopher = CreatePhilosopher<PhilosopherB>(strategy, config, leftForkMock.Object, rightForkMock.Object);

        leftForkMock.Setup(f => f.IsTakenBy(philosopher)).Returns(true);

        // Act & Assert
        Assert.True(strategy.HasLeftFork(philosopher));
    }

    [Fact]
    public void Strategy_HasRightFork_ReturnsCorrectValue()
    {
        // Arrange
        var strategy = new LeftRightStrategy();
        var leftForkMock = new Mock<IFork>();
        var rightForkMock = new Mock<IFork>();

        var config = new PhilosopherConfiguration();
        var philosopher = CreatePhilosopher<PhilosopherB>(strategy, config, leftForkMock.Object, rightForkMock.Object);

        rightForkMock.Setup(f => f.IsTakenBy(philosopher)).Returns(true);

        // Act & Assert
        Assert.True(strategy.HasRightFork(philosopher));
    }

    [Fact]
    public void Strategy_IsForkLocked_ReturnsCorrectValue()
    {
        // Arrange
        var strategy = new LeftRightStrategy();
        var leftForkMock = new Mock<IFork>();
        var rightForkMock = new Mock<IFork>();

        var config = new PhilosopherConfiguration();
        var philosopher = CreatePhilosopher<PhilosopherB>(strategy, config, leftForkMock.Object, rightForkMock.Object);

        leftForkMock.Setup(f => f.IsLockedBy(philosopher)).Returns(true);

        // Act & Assert
        Assert.True(strategy.IsForkLocked(philosopher));
    }
}

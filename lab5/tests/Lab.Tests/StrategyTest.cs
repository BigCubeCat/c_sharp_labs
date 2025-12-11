using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Interface;
using Interface.Strategy;
using Lab.Core.Strategy;
using Moq;
using Xunit;

namespace Lab.Tests;

public class LeftRightStrategyTest
{
    private Mock<IFork> CreateForkMock()
    {
        var forkMock = new Mock<IFork>();
        forkMock.Setup(f => f.TryTake(It.IsAny<IPhilosopher>()));
        forkMock.Setup(f => f.TryLock(It.IsAny<IPhilosopher>()));
        forkMock.Setup(f => f.UnlockFork());
        forkMock.Setup(f => f.Put());
        forkMock.Setup(f => f.GetInfoString()).Returns("Mock Fork");
        forkMock.Setup(f => f.GetScoreString(It.IsAny<double>())).Returns("Mock Fork Score");
        return forkMock;
    }

    private Mock<IPhilosopher> CreatePhilosopherMock(string name, Mock<IFork> leftFork, Mock<IFork> rightFork)
    {
        var philosopherMock = new Mock<IPhilosopher>();
        philosopherMock.Setup(p => p.Name).Returns(name);
        philosopherMock.Setup(p => p.LeftFork).Returns(leftFork?.Object ?? CreateForkMock().Object);
        philosopherMock.Setup(p => p.RightFork).Returns(rightFork?.Object ?? CreateForkMock().Object);
        return philosopherMock;
    }

    [Fact]
    public void Strategy_ImplementsInterfaces_Correctly()
    {
        // Arrange & Act
        var strategy = new LeftRightStrategy();

        // Assert
        Assert.IsAssignableFrom<IStrategy>(strategy);
        Assert.IsAssignableFrom<ILeftRightStrategy>(strategy);
    }

    [Fact]
    public void TakeRightFork_Always_TakesRightFork()
    {
        // Arrange
        var strategy = new LeftRightStrategy();
        var rightForkMock = CreateForkMock();
        var philosopherMock = CreatePhilosopherMock("Test", null, rightForkMock);

        // Act
        strategy.TakeRightFork(philosopherMock.Object);

        // Assert
        rightForkMock.Verify(f => f.TryTake(philosopherMock.Object), Times.Once);
    }

    [Fact]
    public void TakeLeftFork_Always_TakesLeftFork()
    {
        // Arrange
        var strategy = new LeftRightStrategy();
        var leftForkMock = CreateForkMock();
        var philosopherMock = CreatePhilosopherMock("Test", leftForkMock, null);

        // Act
        strategy.TakeLeftFork(philosopherMock.Object);

        // Assert
        leftForkMock.Verify(f => f.TryTake(philosopherMock.Object), Times.Once);
    }

    [Fact]
    public void LockRightFork_Always_LocksRightFork()
    {
        // Arrange
        var strategy = new LeftRightStrategy();
        var rightForkMock = CreateForkMock();
        var philosopherMock = CreatePhilosopherMock("Test", null, rightForkMock);

        // Act
        strategy.LockRightFork(philosopherMock.Object);

        // Assert
        rightForkMock.Verify(f => f.TryLock(philosopherMock.Object), Times.Once);
    }

    [Fact]
    public void LockLeftFork_Always_LocksLeftFork()
    {
        // Arrange
        var strategy = new LeftRightStrategy();
        var leftForkMock = CreateForkMock();
        var philosopherMock = CreatePhilosopherMock("Test", leftForkMock, null);

        // Act
        strategy.LockLeftFork(philosopherMock.Object);

        // Assert
        leftForkMock.Verify(f => f.TryLock(philosopherMock.Object), Times.Once);
    }
}

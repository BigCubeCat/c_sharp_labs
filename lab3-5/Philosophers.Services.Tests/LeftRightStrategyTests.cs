using Interface;
using Moq;
using Philosophers.Services.Strategy;

namespace Philosophers.Services.Tests;

public class LeftRightStrategyTests()
{
    ///
    /// TakeFork function tests
    ///
    
    [Fact]
    public void TakeFork_DoubleCall_ShouldTakeLeftForkBeforeLeft()
    {
        // Arrange
        var leftForkMock = new Mock<IFork>();
        var philosopherMock1 = new Mock<IPhilosopher>();

        var rightForkMock = new Mock<IFork>();
        var philosopherMock2 = new Mock<IPhilosopher>();

        var leftRightStrategy = new LeftRightStrategy();

        leftForkMock.Setup(f => f.TryTake(philosopherMock1.Object));
        rightForkMock.Setup(f => f.TryTake(philosopherMock2.Object));

        philosopherMock1.Setup(p => p.LeftFork).Returns(leftForkMock.Object);
        philosopherMock2.Setup(p => p.RightFork).Returns(rightForkMock.Object);

        //Act
        leftRightStrategy.TakeFork(philosopherMock1.Object);
        leftRightStrategy.TakeFork(philosopherMock2.Object);

        //Assert
        philosopherMock1.Verify(p => p.LeftFork, Times.AtLeastOnce);
        leftForkMock.Verify(f => f.TryTake(It.IsAny<IPhilosopher>()), Times.Once);

        philosopherMock2.Verify(p => p.RightFork, Times.AtLeastOnce);
        rightForkMock.Verify(f => f.TryTake(It.IsAny<IPhilosopher>()), Times.Once);
    }

    ///
    /// TakeLeftFork function tests
    ///

    [Fact]
    public void TakeLeftFork_BasicCall_ShouldTakeLeftFork()
    {
        // Arrange
        var leftForkMock = new Mock<IFork>();
        var philosopherMock = new Mock<IPhilosopher>();
        var leftRightStrategy = new LeftRightStrategy();

        leftForkMock.Setup(f => f.TryTake(philosopherMock.Object));

        philosopherMock.Setup(p => p.LeftFork).Returns(leftForkMock.Object);

        // Act
        leftRightStrategy.TakeLeftFork(philosopherMock.Object);

        // Assert
        philosopherMock.Verify(p => p.LeftFork, Times.AtLeastOnce);
        leftForkMock.Verify(f => f.TryTake(It.IsAny<IPhilosopher>()), Times.Once);
    }

    ///
    /// TakeRightFork function tests
    ///

    [Fact]
    public void TakeRightFork_BasicCall_ShouldTakeRightFork()
    {
        // Arrange
        var rightForkMock = new Mock<IFork>();
        var philosopherMock = new Mock<IPhilosopher>();
        var leftRightStrategy = new LeftRightStrategy();

        rightForkMock.Setup(f => f.TryTake(philosopherMock.Object));

        philosopherMock.Setup(p => p.RightFork).Returns(rightForkMock.Object);

        // Act
        leftRightStrategy.TakeRightFork(philosopherMock.Object);

        // Assert
        philosopherMock.Verify(p => p.RightFork, Times.AtLeastOnce);
        rightForkMock.Verify(f => f.TryTake(It.IsAny<IPhilosopher>()), Times.Once);
    }

    ///
    /// LockFork function tests
    ///

    [Fact]
    public void LockFork_DoubleCall_ShouldLockLeftForkBeforeLeft()
    {
        // Arrange
        var leftForkMock = new Mock<IFork>();
        var philosopherMock1 = new Mock<IPhilosopher>();
        var rightForkMock = new Mock<IFork>();
        var philosopherMock2 = new Mock<IPhilosopher>();
        var leftRightStrategy = new LeftRightStrategy();

        leftForkMock.Setup(f => f.TryLock(philosopherMock1.Object));
        rightForkMock.Setup(f => f.TryLock(philosopherMock2.Object));

        philosopherMock1.Setup(p => p.LeftFork).Returns(leftForkMock.Object);
        philosopherMock2.Setup(p => p.RightFork).Returns(rightForkMock.Object);

        //Act
        leftRightStrategy.LockFork(philosopherMock1.Object);
        leftRightStrategy.LockFork(philosopherMock2.Object);

        //Assert
        philosopherMock1.Verify(p => p.LeftFork, Times.AtLeastOnce);
        leftForkMock.Verify(f => f.TryLock(It.IsAny<IPhilosopher>()), Times.Once);

        philosopherMock2.Verify(p => p.RightFork, Times.AtLeastOnce);
        rightForkMock.Verify(f => f.TryLock(It.IsAny<IPhilosopher>()), Times.Once);
    }

    ///
    /// LockLeftFork function tests
    ///

    [Fact]
    public void LockLeftFork_BasicCall_ShouldLockLeftFork()
    {
        // Arrange
        var leftForkMock = new Mock<IFork>();
        var philosopherMock = new Mock<IPhilosopher>();
        var leftRightStrategy = new LeftRightStrategy();

        leftForkMock.Setup(f => f.TryLock(philosopherMock.Object));

        philosopherMock.Setup(p => p.LeftFork).Returns(leftForkMock.Object);

        // Act
        leftRightStrategy.LockLeftFork(philosopherMock.Object);

        // Assert
        philosopherMock.Verify(p => p.LeftFork, Times.AtLeastOnce);
        leftForkMock.Verify(f => f.TryLock(It.IsAny<IPhilosopher>()), Times.Once);
    }

    ///
    /// LockRightFork function tests
    ///

    [Fact]
    public void LockRightFork_BasicCall_ShouldLockRightFork()
    {
        // Arrange
        var rightForkMock = new Mock<IFork>();
        var philosopherMock = new Mock<IPhilosopher>();
        var leftRightStrategy = new LeftRightStrategy();

        rightForkMock.Setup(f => f.TryLock(philosopherMock.Object));

        philosopherMock.Setup(p => p.RightFork).Returns(rightForkMock.Object);

        // Act
        leftRightStrategy.LockRightFork(philosopherMock.Object);

        // Assert
        philosopherMock.Verify(p => p.RightFork, Times.AtLeastOnce);
        rightForkMock.Verify(f => f.TryLock(It.IsAny<IPhilosopher>()), Times.Once);
    }

    ///
    /// UnlockForks function tests
    ///

    [Fact]
    public void UnlockForks_LockTwoForks_ShouldUnlockTwoForks()
    {
        // Arrange
        var leftForkMock = new Mock<IFork>();
        var rightForkMock = new Mock<IFork>();
        var philosopherMock = new Mock<IPhilosopher>();
        var leftRightStrategy = new LeftRightStrategy();

        philosopherMock.Setup(p => p.RightFork).Returns(rightForkMock.Object);
        philosopherMock.Setup(p => p.LeftFork).Returns(leftForkMock.Object);

        leftForkMock.Setup(f => f.UnlockFork());
        leftForkMock.Setup(f => f.IsLockedBy(philosopherMock.Object)).Returns(true);

        rightForkMock.Setup(f => f.UnlockFork());
        rightForkMock.Setup(f => f.IsLockedBy(philosopherMock.Object)).Returns(true);

        // Act
        leftRightStrategy.UnlockForks(philosopherMock.Object);

        // Assert
        philosopherMock.Verify(p => p.RightFork, Times.AtLeastOnce);
        philosopherMock.Verify(p => p.LeftFork, Times.AtLeastOnce);

        leftForkMock.Verify(f => f.UnlockFork(), Times.Once);
        rightForkMock.Verify(f => f.UnlockFork(), Times.Once);

        leftForkMock.Verify(f => f.IsLockedBy(philosopherMock.Object), Times.Once);
        rightForkMock.Verify(f => f.IsLockedBy(philosopherMock.Object), Times.Once);
    }

    [Fact]
    public void UnlockForks_LockLeftFork_ShouldUnlockOnlyLeftFork()
    {
        // Arrange
        var leftForkMock = new Mock<IFork>();
        var rightForkMock = new Mock<IFork>();
        var philosopherMock = new Mock<IPhilosopher>();
        var leftRightStrategy = new LeftRightStrategy();

        philosopherMock.Setup(p => p.RightFork).Returns(rightForkMock.Object);
        philosopherMock.Setup(p => p.LeftFork).Returns(leftForkMock.Object);

        leftForkMock.Setup(f => f.UnlockFork());
        leftForkMock.Setup(f => f.IsLockedBy(philosopherMock.Object)).Returns(true);

        rightForkMock.Setup(f => f.UnlockFork());
        rightForkMock.Setup(f => f.IsLockedBy(philosopherMock.Object)).Returns(false);

        // Act
        leftRightStrategy.UnlockForks(philosopherMock.Object);

        // Assert
        philosopherMock.Verify(p => p.RightFork, Times.AtLeastOnce);
        philosopherMock.Verify(p => p.LeftFork, Times.AtLeastOnce);

        leftForkMock.Verify(f => f.UnlockFork(), Times.Once);
        rightForkMock.Verify(f => f.UnlockFork(), Times.Never);

        leftForkMock.Verify(f => f.IsLockedBy(philosopherMock.Object), Times.Once);
        rightForkMock.Verify(f => f.IsLockedBy(philosopherMock.Object), Times.Once);
    }

    [Fact]
    public void UnlockForks_LockRightFork_ShouldUnlockOnlyRightFork()
    {
        // Arrange
        var leftForkMock = new Mock<IFork>();
        var rightForkMock = new Mock<IFork>();
        var philosopherMock = new Mock<IPhilosopher>();
        var leftRightStrategy = new LeftRightStrategy();

        philosopherMock.Setup(p => p.RightFork).Returns(rightForkMock.Object);
        philosopherMock.Setup(p => p.LeftFork).Returns(leftForkMock.Object);

        leftForkMock.Setup(f => f.UnlockFork());
        leftForkMock.Setup(f => f.IsLockedBy(philosopherMock.Object)).Returns(false);

        rightForkMock.Setup(f => f.UnlockFork());
        rightForkMock.Setup(f => f.IsLockedBy(philosopherMock.Object)).Returns(true);

        // Act
        leftRightStrategy.UnlockForks(philosopherMock.Object);

        // Assert
        philosopherMock.Verify(p => p.RightFork, Times.AtLeastOnce);
        philosopherMock.Verify(p => p.LeftFork, Times.AtLeastOnce);

        leftForkMock.Verify(f => f.UnlockFork(), Times.Never);
        rightForkMock.Verify(f => f.UnlockFork(), Times.Once);

        leftForkMock.Verify(f => f.IsLockedBy(philosopherMock.Object), Times.Once);
        rightForkMock.Verify(f => f.IsLockedBy(philosopherMock.Object), Times.Once);
    }

    [Fact]
    public void UnlockForks_DoNotLockAnyForks_ShouldNotUnlockForks()
    {
        // Arrange
        var leftForkMock = new Mock<IFork>();
        var rightForkMock = new Mock<IFork>();
        var philosopherMock = new Mock<IPhilosopher>();
        var leftRightStrategy = new LeftRightStrategy();

        philosopherMock.Setup(p => p.RightFork).Returns(rightForkMock.Object);
        philosopherMock.Setup(p => p.LeftFork).Returns(leftForkMock.Object);

        leftForkMock.Setup(f => f.UnlockFork());
        leftForkMock.Setup(f => f.IsLockedBy(philosopherMock.Object)).Returns(false);

        rightForkMock.Setup(f => f.UnlockFork());
        rightForkMock.Setup(f => f.IsLockedBy(philosopherMock.Object)).Returns(false);

        // Act
        leftRightStrategy.UnlockForks(philosopherMock.Object);

        // Assert
        philosopherMock.Verify(p => p.RightFork, Times.AtLeastOnce);
        philosopherMock.Verify(p => p.LeftFork, Times.AtLeastOnce);

        leftForkMock.Verify(f => f.UnlockFork(), Times.Never);
        rightForkMock.Verify(f => f.UnlockFork(), Times.Never);

        leftForkMock.Verify(f => f.IsLockedBy(philosopherMock.Object), Times.Once);
        rightForkMock.Verify(f => f.IsLockedBy(philosopherMock.Object), Times.Once);
    }

    ///
    /// PutForks function tests
    ///

    [Fact]
    public void PutForks_HaveTwoForks_ShouldPutTwoForks()
    {
        // Arrange
        var leftForkMock = new Mock<IFork>();
        var rightForkMock = new Mock<IFork>();
        var philosopherMock = new Mock<IPhilosopher>();
        var leftRightStrategy = new LeftRightStrategy();

        philosopherMock.Setup(p => p.RightFork).Returns(rightForkMock.Object);
        philosopherMock.Setup(p => p.LeftFork).Returns(leftForkMock.Object);

        leftForkMock.Setup(f => f.Put());
        leftForkMock.Setup(f => f.IsTakenBy(philosopherMock.Object)).Returns(true);

        rightForkMock.Setup(f => f.Put());
        rightForkMock.Setup(f => f.IsTakenBy(philosopherMock.Object)).Returns(true);

        // Act
        leftRightStrategy.PutForks(philosopherMock.Object);

        // Assert
        philosopherMock.Verify(p => p.RightFork, Times.AtLeastOnce);
        philosopherMock.Verify(p => p.LeftFork, Times.AtLeastOnce);

        leftForkMock.Verify(f => f.Put(), Times.Once);
        rightForkMock.Verify(f => f.Put(), Times.Once);

        leftForkMock.Verify(f => f.IsTakenBy(philosopherMock.Object), Times.Once);
        rightForkMock.Verify(f => f.IsTakenBy(philosopherMock.Object), Times.Once);
    }

    [Fact]
    public void PutForks_HaveLeftFork_ShouldPutOnlyLeftFork()
    {
        // Arrange
        var leftForkMock = new Mock<IFork>();
        var rightForkMock = new Mock<IFork>();
        var philosopherMock = new Mock<IPhilosopher>();
        var leftRightStrategy = new LeftRightStrategy();

        philosopherMock.Setup(p => p.RightFork).Returns(rightForkMock.Object);
        philosopherMock.Setup(p => p.LeftFork).Returns(leftForkMock.Object);

        leftForkMock.Setup(f => f.Put());
        leftForkMock.Setup(f => f.IsTakenBy(philosopherMock.Object)).Returns(true);

        rightForkMock.Setup(f => f.Put());
        rightForkMock.Setup(f => f.IsTakenBy(philosopherMock.Object)).Returns(false);

        // Act
        leftRightStrategy.PutForks(philosopherMock.Object);

        // Assert
        philosopherMock.Verify(p => p.RightFork, Times.AtLeastOnce);
        philosopherMock.Verify(p => p.LeftFork, Times.AtLeastOnce);

        leftForkMock.Verify(f => f.Put(), Times.Once);
        rightForkMock.Verify(f => f.Put(), Times.Never);

        leftForkMock.Verify(f => f.IsTakenBy(philosopherMock.Object), Times.Once);
        rightForkMock.Verify(f => f.IsTakenBy(philosopherMock.Object), Times.Once);
    }

    [Fact]
    public void PutForks_HaveRightFork_ShouldPutOnlyRightFork()
    {
        // Arrange
        var leftForkMock = new Mock<IFork>();
        var rightForkMock = new Mock<IFork>();
        var philosopherMock = new Mock<IPhilosopher>();
        var leftRightStrategy = new LeftRightStrategy();

        philosopherMock.Setup(p => p.RightFork).Returns(rightForkMock.Object);
        philosopherMock.Setup(p => p.LeftFork).Returns(leftForkMock.Object);

        leftForkMock.Setup(f => f.Put());
        leftForkMock.Setup(f => f.IsTakenBy(philosopherMock.Object)).Returns(false);

        rightForkMock.Setup(f => f.Put());
        rightForkMock.Setup(f => f.IsTakenBy(philosopherMock.Object)).Returns(true);

        // Act
        leftRightStrategy.PutForks(philosopherMock.Object);

        // Assert
        philosopherMock.Verify(p => p.RightFork, Times.AtLeastOnce);
        philosopherMock.Verify(p => p.LeftFork, Times.AtLeastOnce);

        leftForkMock.Verify(f => f.Put(), Times.Never);
        rightForkMock.Verify(f => f.Put(), Times.Once);

        leftForkMock.Verify(f => f.IsTakenBy(philosopherMock.Object), Times.Once);
        rightForkMock.Verify(f => f.IsTakenBy(philosopherMock.Object), Times.Once);
    }

    [Fact]
    public void PutForks_DoNotHaveAnyForks_ShouldNotPutForks()
    {
        // Arrange
        var leftForkMock = new Mock<IFork>();
        var rightForkMock = new Mock<IFork>();
        var philosopherMock = new Mock<IPhilosopher>();
        var leftRightStrategy = new LeftRightStrategy();

        philosopherMock.Setup(p => p.RightFork).Returns(rightForkMock.Object);
        philosopherMock.Setup(p => p.LeftFork).Returns(leftForkMock.Object);

        leftForkMock.Setup(f => f.Put());
        leftForkMock.Setup(f => f.IsTakenBy(philosopherMock.Object)).Returns(false);

        rightForkMock.Setup(f => f.Put());
        rightForkMock.Setup(f => f.IsTakenBy(philosopherMock.Object)).Returns(false);

        // Act
        leftRightStrategy.PutForks(philosopherMock.Object);

        // Assert
        philosopherMock.Verify(p => p.RightFork, Times.AtLeastOnce);
        philosopherMock.Verify(p => p.LeftFork, Times.AtLeastOnce);

        leftForkMock.Verify(f => f.Put(), Times.Never);
        rightForkMock.Verify(f => f.Put(), Times.Never);

        leftForkMock.Verify(f => f.IsTakenBy(philosopherMock.Object), Times.Once);
        rightForkMock.Verify(f => f.IsTakenBy(philosopherMock.Object), Times.Once);
    }

    ///
    /// HasLeftFork function tests
    ///

    [Fact]
    public void HasLeftFork_ReallyHasLeftFork_ReturnsTrue()
    {
        // Arrange
        var leftForkMock = new Mock<IFork>();
        var philosopherMock = new Mock<IPhilosopher>();
        var leftRightStrategy = new LeftRightStrategy();

        philosopherMock.Setup(p => p.LeftFork).Returns(leftForkMock.Object);
        leftForkMock.Setup(f => f.IsTakenBy(philosopherMock.Object)).Returns(true);

        // Act
        var result = leftRightStrategy.HasLeftFork(philosopherMock.Object);

        // Accept
        Assert.True(result);

        philosopherMock.Verify(p => p.LeftFork, Times.AtLeastOnce);
        leftForkMock.Verify(f => f.IsTakenBy(philosopherMock.Object), Times.Once);
    }

    [Fact]
    public void HasLeftFork_DoesNotHaveLeftFork_ReturnsFalse()
    {
        // Arrange
        var leftForkMock = new Mock<IFork>();
        var philosopherMock = new Mock<IPhilosopher>();
        var leftRightStrategy = new LeftRightStrategy();

        philosopherMock.Setup(p => p.LeftFork).Returns(leftForkMock.Object);
        leftForkMock.Setup(f => f.IsTakenBy(philosopherMock.Object)).Returns(false);

        // Act
        var result = leftRightStrategy.HasLeftFork(philosopherMock.Object);

        // Accept
        Assert.False(result);

        philosopherMock.Verify(p => p.LeftFork, Times.AtLeastOnce);
        leftForkMock.Verify(f => f.IsTakenBy(philosopherMock.Object), Times.Once);
    }

    ///
    /// HasRightFork function tests
    ///

    [Fact]
    public void HasRightFork_ReallyHasRightFork_ReturnsTrue()
    {
        // Arrange
        var rightForkMock = new Mock<IFork>();
        var philosopherMock = new Mock<IPhilosopher>();
        var leftRightStrategy = new LeftRightStrategy();

        philosopherMock.Setup(p => p.RightFork).Returns(rightForkMock.Object);
        rightForkMock.Setup(f => f.IsTakenBy(philosopherMock.Object)).Returns(true);

        // Act
        var result = leftRightStrategy.HasRightFork(philosopherMock.Object);

        // Accept
        Assert.True(result);

        philosopherMock.Verify(p => p.RightFork, Times.AtLeastOnce);
        rightForkMock.Verify(f => f.IsTakenBy(philosopherMock.Object), Times.Once);
    }

    [Fact]
    public void HasRightFork_DoesNotHaveRightFork_ReturnsFalse()
    {
        // Arrange
        var rightForkMock = new Mock<IFork>();
        var philosopherMock = new Mock<IPhilosopher>();
        var leftRightStrategy = new LeftRightStrategy();

        philosopherMock.Setup(p => p.RightFork).Returns(rightForkMock.Object);
        rightForkMock.Setup(f => f.IsTakenBy(philosopherMock.Object)).Returns(false);

        // Act
        var result = leftRightStrategy.HasRightFork(philosopherMock.Object);

        // Accept
        Assert.False(result);

        philosopherMock.Verify(p => p.RightFork, Times.AtLeastOnce);
        rightForkMock.Verify(f => f.IsTakenBy(philosopherMock.Object), Times.Once);
    }

    ///
    /// IsForkLocked function tests
    ///

    [Fact]
    public void IsForkLocked_LockTwoForks_ReturnsTrue()
    {
        // Arrange
        var leftForkMock = new Mock<IFork>();
        var rightForkMock = new Mock<IFork>();
        var philosopherMock = new Mock<IPhilosopher>();
        var leftRightStrategy = new LeftRightStrategy();

        philosopherMock.Setup(p => p.RightFork).Returns(rightForkMock.Object);
        philosopherMock.Setup(p => p.LeftFork).Returns(leftForkMock.Object);

        leftForkMock.Setup(f => f.IsLockedBy(philosopherMock.Object)).Returns(true);
        rightForkMock.Setup(f => f.IsLockedBy(philosopherMock.Object)).Returns(true);

        // Act
        var result = leftRightStrategy.IsForkLocked(philosopherMock.Object);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsForkLocked_LockOnlyLeftFork_ReturnsTrue()
    {
        // Arrange
        var leftForkMock = new Mock<IFork>();
        var rightForkMock = new Mock<IFork>();
        var philosopherMock = new Mock<IPhilosopher>();
        var leftRightStrategy = new LeftRightStrategy();

        philosopherMock.Setup(p => p.RightFork).Returns(rightForkMock.Object);
        philosopherMock.Setup(p => p.LeftFork).Returns(leftForkMock.Object);

        leftForkMock.Setup(f => f.IsLockedBy(philosopherMock.Object)).Returns(true);
        rightForkMock.Setup(f => f.IsLockedBy(philosopherMock.Object)).Returns(false);

        // Act
        var result = leftRightStrategy.IsForkLocked(philosopherMock.Object);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsForkLocked_LockOnlyRightFork_ReturnsTrue()
    {
        // Arrange
        var leftForkMock = new Mock<IFork>();
        var rightForkMock = new Mock<IFork>();
        var philosopherMock = new Mock<IPhilosopher>();
        var leftRightStrategy = new LeftRightStrategy();

        philosopherMock.Setup(p => p.RightFork).Returns(rightForkMock.Object);
        philosopherMock.Setup(p => p.LeftFork).Returns(leftForkMock.Object);

        leftForkMock.Setup(f => f.IsLockedBy(philosopherMock.Object)).Returns(false);
        rightForkMock.Setup(f => f.IsLockedBy(philosopherMock.Object)).Returns(true);

        // Act
        var result = leftRightStrategy.IsForkLocked(philosopherMock.Object);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsForkLocked_DoNotLockAnyForks_ReturnsFalse()
    {
        // Arrange
        var leftForkMock = new Mock<IFork>();
        var rightForkMock = new Mock<IFork>();
        var philosopherMock = new Mock<IPhilosopher>();
        var leftRightStrategy = new LeftRightStrategy();

        philosopherMock.Setup(p => p.RightFork).Returns(rightForkMock.Object);
        philosopherMock.Setup(p => p.LeftFork).Returns(leftForkMock.Object);

        leftForkMock.Setup(f => f.IsLockedBy(philosopherMock.Object)).Returns(false);
        rightForkMock.Setup(f => f.IsLockedBy(philosopherMock.Object)).Returns(false);

        // Act
        var result = leftRightStrategy.IsForkLocked(philosopherMock.Object);

        // Assert
        Assert.False(result);
    }
}

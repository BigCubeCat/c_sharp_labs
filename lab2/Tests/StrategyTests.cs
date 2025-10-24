// using Xunit;
// using Moq;
// using Interface.Strategy;
// using Interface;

// public class NaiveStrategyTests
// {
//     [Fact]
//     public void NaiveStrategy_Takes_Left_Then_Right_Fork()
//     {
//         // Arrange
//         var leftFork = new Mock<IFork>();
//         var rightFork = new Mock<IFork>();
//         var strategy = new NaiveStrategy(); // если есть реализация

//         // Act
//         strategy.TryAcquire(leftFork.Object, rightFork.Object);

//         // Assert
//         leftFork.Verify(f => f.Take(), Times.Once);
//         rightFork.Verify(f => f.Take(), Times.Once);
//     }
// }

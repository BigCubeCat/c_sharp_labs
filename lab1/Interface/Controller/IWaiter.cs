namespace Interface.Controller;

/// <summary>
/// Interface for a waiter (central controller) responsible for coordinating forks between philosophers.
/// </summary>
public interface IWaiter
{
    /// <summary>
    /// Factory method to create a waiter for a given set of clients and forks.
    /// </summary>
    /// <param name="philosophers">List of philosopher controllers.</param>
    /// <param name="forks">List of forks (controller implementations).</param>
    /// <param name="isDeadlockConfigure">Flag that influences initial handedness configuration.</param>
    /// <returns>Waiter instance.</returns>
    abstract static IWaiter Create(List<IPhilosopherController> philosophers, List<IForkController> forks, bool isDeadlockConfigure);
    /// <summary>
    ///     Event fired when waiter grants right fork to a philosopher (notifies the philosopher).
    /// </summary>
    event EventHandler<IPhilosopherController> YouHasRightForkNotify;
    
    /// <summary>
    ///     Event fired when waiter grants left fork to a philosopher.
    /// </summary>
    event EventHandler<IPhilosopherController> YouHasLeftForkNotify;
    
    /// <summary>
    ///     Advance waiter internal logic by one simulation step.
    /// </summary>
    void Step();
}

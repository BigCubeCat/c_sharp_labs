namespace Interface.Strategy;

/// <summary>
/// Strategy-mode fork interface. Provides TryTake semantics for decentralized algorithm.
/// </summary>
public interface IForkStrategy : IFork
{
    /// <summary>
    /// Try to take the fork by a philosopher without throwing on failure.
    /// </summary>
    /// <param name="philosopher">Philosopher attempting to take the fork.</param>
    /// <returns><see langword="true"/> when take succeeded; otherwise <see langword="false"/>.</returns>
    bool TryTake(IPhilosopher philosopher);
}


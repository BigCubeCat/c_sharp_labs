using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Interface;

/// <summary>
/// Represents a fork in the Dining Philosophers simulation.
/// Contains properties and operations shared by different fork implementations.
/// </summary>
public interface IFork
{
    /// <summary>
    /// Total time (in simulation steps) the fork was used (taken by a philosopher).
    /// </summary>
    int UsedTime { get; }
    /// <summary>
    ///     Total time (in simulation steps) the fork was blocked (locked but not taken).
    ///     Implementations may set this to 0 if blocking concept is not used.
    /// </summary>
    int BlockTime { get; }
    /// <summary>
    ///     Total time (in simulation steps) the fork was available (free).
    /// </summary>
    int AvailableTime { get; }
    
    /// <summary>
    ///     Factory method to create a fork implementation instance.
    ///     Implementations should override this static method and return a concrete <see cref="IFork"/>.
    /// </summary>
    /// <param name="number">Identifier (index) of the fork.</param>
    /// <returns>New <see cref="IFork"/> instance.</returns>
    virtual static IFork Create(int number)
    {
        throw new NotImplementedException("Create function not implemented here");
    }
    /// <summary>
    ///     Current owner (philosopher) of the fork or <c>null</c> if fork is free.
    /// </summary>
    IPhilosopher? Owner { get; protected internal set; }
    /// <summary>
    ///     Put the fork back (release it). Behaviour depends on implementation:
    ///     in controller mode this may also release lock state.
    /// </summary>
    void Put();
    /// <summary>
    ///     Print human-readable information about the fork (used in console output).
    /// </summary>
    void PrintInfo();
    
    /// <summary>
    ///     Print statistics (score) for the fork given total simulation time.
    /// </summary>
    /// <param name="simulationTime">Total simulation time (in steps).</param>
    void PrintScore(double simulationTime);
    /// <summary>
    ///     Progress simulation one step for fork (update counters).
    /// </summary>
    void Step();
}


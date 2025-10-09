using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Interface;

/// <summary>
/// Common interface for philosopher entities.
/// Provides minimal contract used by different simulation modes.
/// </summary>
public interface IPhilosopher
{
    /// <summary>
    /// How many times the philosopher finished eating (count of eaten portions).
    /// </summary>
    int CountEatingFood { get; }

    /// <summary>
    ///     Total time (in simulation steps) the philosopher spent in hungry state.
    /// </summary>
    int HungryTime { get; }

    /// <summary>
    ///     Factory method to create an <see cref="IPhilosopher"/> from data transfer object.
    /// </summary>
    /// <param name="philosopherDTO">DTO containing philosopher configuration.</param>
    /// <returns>Concrete philosopher instance.</returns>
    virtual static IPhilosopher Create(PhilosopherDTO philosopherDTO)
    {
        throw new NotImplementedException("Create function not implemented here");
    }

    /// <summary>
    ///     Philosopher display name.
    /// </summary>
    string Name { get; protected internal set; }

    /// <summary>
    ///     Advance philosopher state by one simulation step.
    /// </summary>
    void Step();

    /// <summary>
    ///     Print human-readable state information to console.
    /// </summary>
    void PrintInfo();

    /// <summary>
    ///     Print performance statistics for the philosopher.
    /// </summary>
    /// <param name="simulationTime">Total simulation time (in steps).</param>
    void PrintScore(double simulationTime);

    /// <summary>
    ///     Returns <see langword="true"/> if philosopher is currently eating.
    /// </summary>
    /// <returns><see langword="true"/> when state is Eating; otherwise <see langword="false"/>.</returns>
    bool IsEating();
}
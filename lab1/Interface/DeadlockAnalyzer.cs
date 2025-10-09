using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Interface;

/// <summary>
/// Helper class to analyze deadlock situation in the philosophers simulation.
/// </summary>
public abstract class DeadlockAnalyzer
{
    /// <summary>
    /// Checks whether a deadlock occurred.
    /// Deadlock is considered to be present when all forks are taken (no free fork)
    /// and none of the philosophers is currently eating.
    /// </summary>
    /// <param name="philosophers">List of philosophers participating in the simulation.</param>
    /// <param name="forks">List of forks participating in the simulation.</param>
    /// <returns><see langword="true"/> if deadlock is detected; otherwise <see langword="false"/>.</returns>
    public static bool IsDeadlock(List<IPhilosopher> philosophers, List<IFork> forks)
    {
        int countUsingForks = 0;
        foreach (var fork in forks)
        {
            if (fork.Owner != null) ++countUsingForks;
        }

        if (countUsingForks == forks.Count)
        {
            foreach (var philosopher in philosophers)
            {
                if (philosopher.IsEating()) return false;
            }

            return true;
        }

        return false;
    }
}
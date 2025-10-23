using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Interface;

public abstract class DeadlockAnalyzer
{
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

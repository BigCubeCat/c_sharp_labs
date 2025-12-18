using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Interface;
using Interface.Strategy;

namespace Philosophers.Services.Strategy;

public class LeftRightStrategy : ILeftRightStrategy
{
    private IPhilosopher? _leftHandedPhilosopher;

    public void TakeFork(IPhilosopher philosopher)
    {
        _leftHandedPhilosopher ??= philosopher; // probable this comparison is redundant 
        
        if (philosopher == _leftHandedPhilosopher)
        {
            philosopher.LeftFork.TryTake(philosopher);
        }
        else
        {
            philosopher.RightFork.TryTake(philosopher);
        }
    }

    public void TakeRightFork(IPhilosopher philosopher)
    {
        philosopher.RightFork.TryTake(philosopher);
    }

    public void TakeLeftFork(IPhilosopher philosopher)
    {
        philosopher.LeftFork.TryTake(philosopher);
    }

    public void LockFork(IPhilosopher philosopher)
    {
        _leftHandedPhilosopher ??= philosopher;

        if (philosopher == _leftHandedPhilosopher)
        {
            philosopher.LeftFork.TryLock(philosopher);
        }
        else
        {
            philosopher.RightFork.TryLock(philosopher);
        }
    }

    public void LockRightFork(IPhilosopher philosopher)
    {
        philosopher.RightFork.TryLock(philosopher);
    }

    public void LockLeftFork(IPhilosopher philosopher)
    {
        philosopher.LeftFork.TryLock(philosopher);
    }

    public void UnlockForks(IPhilosopher philosopher)
    {
        if (philosopher.LeftFork.IsLockedBy(philosopher))
        {
            philosopher.LeftFork.UnlockFork();
        }

        if (philosopher.RightFork.IsLockedBy(philosopher))
        {
            philosopher.RightFork.UnlockFork();
        }
    }

    public void PutForks(IPhilosopher philosopher)
    {
        if (philosopher.LeftFork.IsTakenBy(philosopher))
        {
            philosopher.LeftFork.Put();
        }

        if (philosopher.RightFork.IsTakenBy(philosopher))
        {
            philosopher.RightFork.Put();
        }
    }

    public bool HasLeftFork(IPhilosopher philosopher)
    {
        return philosopher.LeftFork.IsTakenBy(philosopher);
    }

    public bool HasRightFork(IPhilosopher philosopher)
    {
        return philosopher.RightFork.IsTakenBy(philosopher);
    }

    public bool IsForkLocked(IPhilosopher philosopher)
    {
        return philosopher.LeftFork.IsLockedBy(philosopher) || philosopher.RightFork.IsLockedBy(philosopher);
    }
}

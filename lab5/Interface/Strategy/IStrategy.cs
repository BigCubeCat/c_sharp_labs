using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Interface.Strategy;

public interface IStrategy
{
    void TakeFork(IPhilosopher philosopher);
    void LockFork(IPhilosopher philosopher);
    void LockRightFork(IPhilosopher philosopher);
    void LockLeftFork(IPhilosopher philosopher);
    void TakeRightFork(IPhilosopher philosopher);
    void TakeLeftFork(IPhilosopher philosopher);
    void UnlockForks(IPhilosopher philosopher);
    void PutForks(IPhilosopher philosopher);
    bool HasLeftFork(IPhilosopher philosopher);
    bool HasRightFork(IPhilosopher philosopher);
    bool IsForkLocked(IPhilosopher philosopher);
}

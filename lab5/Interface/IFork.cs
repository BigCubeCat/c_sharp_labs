using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Interface;

public interface IFork: IAccessible
{
    long UsedTime { get; }
    long BlockTime { get; }
    long AvailableTime { get; }
    IPhilosopher? Owner { get; }
    void Put();
    void TryTake(IPhilosopher philosopher);
    void TryLock(IPhilosopher philosopher);
    bool IsLockedBy(IPhilosopher philosopher);
    bool IsTakenBy(IPhilosopher philosopher);
    void UnlockFork();
}

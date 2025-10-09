using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Interface.Controller;

/// <summary>
/// Controller-mode extension for <see cref="Interface.IFork"/> providing explicit lock/take semantics.
/// </summary>
public interface IForkController : IFork
{
    /// <summary>
    /// Take the fork by the specified philosopher. Throws if fork is already taken or locked by someone else.
    /// </summary>
    /// <param name="philosopher">Philosopher that takes the fork.</param>
    void Take(IPhilosopher philosopher);
    /// <summary>
    ///     Lock the fork for the specified philosopher (reserve without transferring ownership).
    /// </summary>
    /// <param name="philosopher">Philosopher that locks the fork.</param>
    void Lock(IPhilosopher philosopher);
    /// <summary>
    ///     Returns <see langword="true"/> when fork is locked (reserved).
    /// </summary>
    /// <returns>Lock state.</returns>
    bool IsLocked();
    /// <summary>
    ///     Returns <see langword="true"/> when fork is locked by the given philosopher.
    /// </summary>
    /// <param name="philosopher">Philosopher to check against.</param>
    /// <returns>True if locked by the philosopher.</returns>
    bool IsLockedBy(IPhilosopher philosopher);
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interface;
using Interface.Controller;

namespace Src.Controller;

/// <summary>
/// Controller-mode implementation of fork with explicit locking semantics.
/// </summary>
public class Fork: IForkController
{
    private bool _isTaken;
    private bool _isLocked;
    private readonly int _number;
    public IPhilosopher? Owner { get; set; }
    public IPhilosopher? Locker { get; set; }
    public int UsedTime { get; private set; }
    public int BlockTime { get; private set; }
    public int AvailableTime { get; private set; }

    /// <summary>
    ///     Factory method used by loader to instantiate a controller-mode fork.
    /// </summary>
    /// <param name="number">Fork identifier.</param>
    /// <returns>New <see cref="IFork"/> instance.</returns>
    public static IFork Create(int number)
    {
        return new Fork(number);
    }

    
    /// <summary>
    ///     Create new fork instance.
    /// </summary>
    /// <param name="number">Fork identifier for human-readable output.</param>
    public Fork(int number)
    {
        _isTaken = false;
        _isLocked = false;
        Owner = null;
        Locker = null;
        _number = number;
    }

    /// <summary>
    ///     Try to take fork non-atomically. Returns false if already taken.
    ///     This method is used by strategy-mode forks as well; controller fork exposes Take/Lock too.
    /// </summary>
    /// <param name="philosopher">Philosopher attempting to take the fork.</param>
    /// <returns>True if take succeeded.</returns>
    public bool TryTake(IPhilosopher philosopher)
    {
        if (_isTaken)
            return false;

        Owner = philosopher;
        _isTaken = true;
        return true;
    }

    /// <summary>
    ///     Put the fork back (release ownership and lock).
    /// </summary>
    public void Put()
    {
        if (!_isTaken && !_isLocked)
            throw new ApplicationException("Try to put not taken fork");

        Owner = null;
        Locker = null;
        _isTaken = false;
        _isLocked = false;
    }

    /// <summary>
    ///     Take the fork: requires that fork is locked by the same philosopher.
    ///     If conditions are not met an exception will be thrown.
    /// </summary>
    /// <param name="philosopher">Philosopher taking the fork.</param>
    public void Take(IPhilosopher philosopher)
    {
        if (_isTaken || !_isLocked || (_isLocked && Locker != philosopher))
        {
            string message = string.Format("{0} try to take already taken fork {1} by {2}, locked by {3}",
                philosopher.Name, _number, Owner?.Name, Locker?.Name);
            throw new ApplicationException(message);
        }

        Locker = philosopher;
        Owner = philosopher;
        _isTaken = true;
        _isLocked = true;
    }

    /// <summary>
    ///     Print information about current fork state to console.
    /// </summary>
    public void PrintInfo()
    {
        var builder = new StringBuilder();
        builder.AppendFormat("Fork-{0}: ", _number);

        if (Owner is null)
            builder.Append("Available");
        else
            builder.AppendFormat("In Use (used by {0})", Owner.Name);

        Console.WriteLine(builder.ToString());
    }

    /// <summary>
    ///     Lock the fork for the given philosopher (reserve it).
    /// </summary>
    /// <param name="philosopher">Philosopher that requests lock.</param>
    public void Lock(IPhilosopher philosopher)
    {
        if (_isLocked)
        {
            string message = string.Format("{0} try to locked already locked fork {1} by {2}",
                philosopher.Name, _number, Locker?.Name);
            throw new ApplicationException(message);
        }

        Locker = philosopher;
        _isLocked = true;
    }

    /// <summary>
    ///     Returns whether the fork is currently locked.
    /// </summary>
    public bool IsLocked()
    {
        return _isLocked;
    }

    /// <summary>
    ///     Returns whether this fork is locked by the specified philosopher.
    /// </summary>
    /// <param name="philosopher">Philosopher to check.</param>
    /// <returns>True if locked by the philosopher.</returns>
    public bool IsLockedBy(IPhilosopher philosopher)
    {
        return _isLocked && Locker == philosopher;
    }

    
    /// <summary>
    ///     Advance internal counters by one simulation step.
    /// </summary>
    public void Step()
    {
        if (_isTaken) ++UsedTime;
        else if (_isLocked) ++BlockTime;
        else ++AvailableTime;
    }
    
    /// <summary>
    ///     Print fork statistics (usage percentages).
    /// </summary>
    /// <param name="simulationTime">Total simulation time used to compute percentages.</param>
    public void PrintScore(double simulationTime)
    {
        var builder = new StringBuilder();
        builder.AppendFormat("Fork-{0}: used {1}%, block {2}%, available {3}%",
            _number, UsedTime / simulationTime, BlockTime / simulationTime, AvailableTime / simulationTime);
        Console.WriteLine(builder.ToString());
    }
}


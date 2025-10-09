using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Interface.Controller;

/// <summary>
/// Controller-mode philosopher interface extending base philosopher with events used to interact with waiter.
/// </summary>
public interface IPhilosopherController : IPhilosopher
{
    /// <summary>
    /// Waiter assigned to this philosopher (null until waiter added).
    /// </summary>
    IWaiter? Waiter { get; protected internal set; }
    
    /// <summary>
    ///     Raised when philosopher becomes hungry.
    /// </summary>
    event EventHandler IAmHungryNotify;
    
    /// <summary>
    ///     Raised when philosopher needs left fork.
    /// </summary>
    event EventHandler INeedLeftForkNotify;
    
    /// <summary>
    ///     Raised when philosopher needs right fork.
    /// </summary>
    event EventHandler INeedRightForkNotify;
    
    /// <summary>
    ///     Raised by philosopher to request to take left fork (notifies waiter).
    /// </summary>
    event EventHandler ICanTakeLeftForkNotify;
    
    /// <summary>
    ///     Raised by philosopher to request to take right fork (notifies waiter).
    /// </summary>
    event EventHandler ICanTakeRightForkNotify;
    
    /// <summary>
    ///     Raised when philosopher finished eating (is full).
    /// </summary>
    event EventHandler IAmFullNotify;
    
    /// <summary>
    ///     Adds waiter and subscribes philosopher to waiter's notifications.
    /// </summary>
    /// <param name="waiter">Waiter instance.</param>
    public void AddWaiterAndSubscribeOnHisEvents(IWaiter waiter);
}


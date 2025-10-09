using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interface;
using Interface.Controller;

namespace Src.Controller;

/// <summary>
/// Controller-mode philosopher implementation.
/// Uses events to interact with waiter and supports locking/take semantics delegated to waiter.
/// </summary>
public class Philosopher : IPhilosopherController
{
    private PhilosopherStates _state;
    private Actions _action;
    private readonly int _eatingTime;
    private readonly int _takeForkTime;
    private readonly int _thinkingTime;
    private readonly int _putForkTimeout;
    private int _counter;
    private bool _leftForkTaken;
    private bool _rightForkTaken;

    public string Name { get; set; }
    public IWaiter? Waiter { get; set; }
    public int HungryTime { get; private set; }
    public int CountEatingFood { get; private set; }
    public event EventHandler IAmHungryNotify;
    public event EventHandler IAmFullNotify;
    public event EventHandler INeedLeftForkNotify;
    public event EventHandler INeedRightForkNotify;
    public event EventHandler ICanTakeLeftForkNotify;
    public event EventHandler ICanTakeRightForkNotify;
    
    /// <summary>
    ///     Factory method creating controller-mode philosopher from DTO.
    /// </summary>
    /// <param name="philosopherDto">Configuration DTO.</param>
    /// <returns>New philosopher instance.</returns>
    public static IPhilosopher Create(PhilosopherDTO philosopherDto)
    {
        return new Philosopher
            (
                philosopherDto.Name,
                philosopherDto.EatingTime,
                philosopherDto.TakeForkTime,
                philosopherDto.ThinkingTime,
                philosopherDto.PutForkTimeout
            );
    }

    public Philosopher(string name, int eatingTime, int takeForkTime, int thinkingTime, int putForkTimeout)
    {
        Name = name;

        _state = PhilosopherStates.Thinking;
        _eatingTime = eatingTime;
        _takeForkTime = takeForkTime;
        _thinkingTime = thinkingTime;
        _putForkTimeout = putForkTimeout; // maybe it useless

        _counter = 0;
        HungryTime = 0;
        CountEatingFood = 0;
        _leftForkTaken = false;
        _rightForkTaken = false;
    }

    public void AddWaiterAndSubscribeOnHisEvents(IWaiter waiter)
    {
        Waiter = waiter;

        Waiter.YouHasRightForkNotify += IHaveRightForkHandler;
        Waiter.YouHasLeftForkNotify += IHaveLeftForkHandler;
    }

    private void IHaveRightForkHandler(object? sender, IPhilosopherController philosopher)
    {
        if ((_action != Actions.TryTakeFork || _action != Actions.TryTakeRightFork) && philosopher != this)
            return;

        _counter = 0;

        _action = Actions.TryTakeRightFork;
        _state = PhilosopherStates.TakeRightFork;
    }

    private void IHaveLeftForkHandler(object? sender, IPhilosopherController philosopher)
    {
        if ((_action != Actions.TryTakeFork || _action != Actions.TryTakeLeftFork) && philosopher != this)
            return;

        _counter = 0;

        _action = Actions.TryTakeLeftFork;
        _state = PhilosopherStates.TakeLeftFork;
    }

    public bool IsEating()
    {
        return _state == PhilosopherStates.Eating;
    }

    public void PrintInfo()
    {
        var builder = new StringBuilder(Name);
        _ = builder.AppendFormat(": {0} (Action = {1}, {2} steps left), eating: {3}", _state, _action, _counter, CountEatingFood);
        Console.WriteLine(builder.ToString());
    }

    public void PrintScore(double simulationTime)
    {
        var builder = new StringBuilder(Name);
        _ = builder.AppendFormat(": bandwidth {0} (eat / (1000 * steps)) ", CountEatingFood / simulationTime);
        _ = builder.AppendFormat(": hungry {0} (steps)", HungryTime);
        Console.WriteLine(builder.ToString());
    }

    public void Step()
    {
        switch (_state)
        {
            case PhilosopherStates.Thinking:
                {
                    ProcessThinkingState();
                    break;
                }
            case PhilosopherStates.Hungry:
                {
                    ProcessHungryState();
                    break;
                }
            case PhilosopherStates.TakeLeftFork:
                {
                    ProcessTakeLeftForkState();
                    break;
                }
            case PhilosopherStates.TakeRightFork:
                {
                    ProcessTakeRightForkState();
                    break;
                }
            case PhilosopherStates.Eating:
                {
                    ProcessEatingState();
                    break;
                }
            default: throw new ApplicationException("Unknown philosopher state");
        }
    }

    private void ProcessThinkingState()
    {
        if (_counter < _thinkingTime)
        {
            ++_counter;
            _action = Actions.None;
            return;
        }

        _counter = 0;
        _state = PhilosopherStates.Hungry;
        _action = Actions.TryTakeFork;
        IAmHungryNotify(this, EventArgs.Empty);
    }

    private void ProcessEatingState()
    {
        if (_counter < _eatingTime)
        {
            ++_counter;
            return;
        }

        _counter = 0;
        ++CountEatingFood;

        _state = PhilosopherStates.Thinking;
        _action = Actions.ReleaseForks;
        _rightForkTaken = false;
        _leftForkTaken = false;

        IAmFullNotify(this, EventArgs.Empty);
    }

    private void ProcessTakeLeftForkState()
    {
        if (_counter < _takeForkTime)
        {
            ++_counter;
            _action = Actions.TryTakeLeftFork;
            return;
        }

        ICanTakeLeftForkNotify(this, EventArgs.Empty);

        _counter = 0;

        if (_rightForkTaken)
        {
            _rightForkTaken = false;
            _state = PhilosopherStates.Eating;
            _action = Actions.None;
        }
        else
        {
            _leftForkTaken = true;
            _state = PhilosopherStates.Hungry;
            _action = Actions.TryTakeRightFork;

            INeedRightForkNotify(this, EventArgs.Empty);
        }
    }

    private void ProcessTakeRightForkState()
    {
        if (_counter < _takeForkTime)
        {
            ++_counter;
            _action = Actions.TryTakeRightFork;
            return;
        }

        ICanTakeRightForkNotify(this, EventArgs.Empty);

        _counter = 0;

        if (_leftForkTaken)
        {
            _leftForkTaken = false;
            _state = PhilosopherStates.Eating;
            _action = Actions.None;
        }
        else
        {
            _rightForkTaken = true;
            _state = PhilosopherStates.Hungry;
            _action = Actions.TryTakeLeftFork;
            INeedLeftForkNotify(this, EventArgs.Empty);
        }
    }

    private void ProcessHungryState()
    {
        ++HungryTime;
        ++_counter;
    }
}


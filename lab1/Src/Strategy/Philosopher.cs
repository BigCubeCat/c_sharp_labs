using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interface;
using Interface.Strategy;

namespace Src.Strategy;

public class Philosopher: IPhilosopherStrategy
{
    private PhilosopherStates _state;
    private Actions _action;
    private readonly int _eatingTime;
    private readonly int _takeForkTime;
    private readonly int _thinkingTime;
    private readonly int _putForkTimeout;
    private int _counter;
    private int _stateTimeCounter;

    public string Name { get; set; }
    public int CountEatingFood { get; private set; }
    public int HungryTime { get; private set; }
    public IForkStrategy? LeftFork { get; set; }
    public IForkStrategy? RightFork { get; set; }
    public bool FirstTakeLeftFork { get; set; }

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
        _stateTimeCounter = 0;
        HungryTime = 0;
        CountEatingFood = 0;
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
        ++_stateTimeCounter;

        if (_counter < _thinkingTime)
        {
            ++_counter;
            _action = Actions.None;
            return;
        }

        _counter = 0;
        _stateTimeCounter = 0;
        _state = PhilosopherStates.Hungry;
        if (FirstTakeLeftFork)
            _action = Actions.TryTakeLeftFork;
        else
            _action = Actions.TryTakeRightFork;
    }

    private void ProcessHungryState()
    {
        ++HungryTime;
        ++_stateTimeCounter;

        if (_counter < _takeForkTime)
        {
            ++_counter;
            if (FirstTakeLeftFork)
                _action = Actions.TryTakeLeftFork;
            else
                _action = Actions.TryTakeLeftFork;

            return;
        }

        _counter = 0;

        if (FirstTakeLeftFork && LeftFork!.TryTake(this))
        {
            _stateTimeCounter = 0;
            _state = PhilosopherStates.TakeLeftFork;
            _action = Actions.TakeLeftFork;
        }
        else if (RightFork!.TryTake(this))
        {
            _stateTimeCounter = 0;
            _state = PhilosopherStates.TakeRightFork;
            _action = Actions.TakeRightFork;
        }
    }

    private void ProcessTakeLeftForkState()
    {
        ++_stateTimeCounter;

        if (_counter < _takeForkTime)
        {
            ++_counter;
            _action = Actions.TryTakeLeftFork;
            return;
        }

        _counter = 0;

        if (RightFork!.TryTake(this))
        {
            _stateTimeCounter = 0;
            _state = PhilosopherStates.Eating;
            _action = Actions.None;
        }
    }

    private void ProcessTakeRightForkState()
    {
        ++_stateTimeCounter;

        if (_counter < _takeForkTime)
        {
            ++_counter;
            _action = Actions.TryTakeRightFork;
        }

        _counter = 0;

        if (LeftFork!.TryTake(this))
        {
            _stateTimeCounter = 0;
            _state = PhilosopherStates.Eating;
            _action = Actions.None;
        }
    }

    private void ProcessEatingState()
    {
        ++_stateTimeCounter;

        if (_counter < _eatingTime)
        {
            ++_counter;
            return;
        }

        _counter = 0;
        _stateTimeCounter = 0;
        ++CountEatingFood;

        if (FirstTakeLeftFork)
        {
            LeftFork!.Put();
            RightFork!.Put();
        }
        else
        {
            RightFork!.Put();
            LeftFork!.Put();
        }

        _state = PhilosopherStates.Thinking;
        _action = Actions.ReleaseForks;
    }

    public void PrintInfo()
    {
        var builder = new StringBuilder(Name);
        _ = builder.AppendFormat(": {0} (Action = {1}, {2} steps left), eating: {3}", _state, _action, _stateTimeCounter, CountEatingFood);
        Console.WriteLine(builder.ToString());
    }

    public void PrintScore(double simulationTime)
    {
        var builder = new StringBuilder(Name);
        _ = builder.AppendFormat(": bandwidth {0}", CountEatingFood / simulationTime);
        _ = builder.AppendFormat(": hungry {0} (steps)", HungryTime);
        Console.WriteLine(builder.ToString());
    }

    public bool IsEating()
    {
        return _state == PhilosopherStates.Eating;
    }
}


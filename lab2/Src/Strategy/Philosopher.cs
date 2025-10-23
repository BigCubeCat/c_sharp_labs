using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Interface;
using Interface.Strategy;

namespace Src.Strategy;

public class Philosopher: IPhilosopherStrategy
{
    private PhilosopherStates _state;
    private readonly int _eatingTime;
    private readonly int _takeForkTime;
    private readonly int _thinkingTime;
    private readonly int _putForkTimeout;
    private int _stateTimer;
    private Thread? _myThread;
    private CancellationTokenSource? _internalCts;
    private readonly object _lockObject = new object();

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

        _stateTimer = 0;
        HungryTime = 0;
        CountEatingFood = 0;
    }

    public void Start(CancellationToken cancellationToken)
    {
        lock (_lockObject)
        {
            if (_myThread != null && _myThread.IsAlive)
                return;

            _internalCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _myThread = new Thread(() => Run(_internalCts.Token)) 
            { 
                IsBackground = true,
                Name = $"Philosopher-{Name}"
            };
            _myThread.Start();
        }
    }

    public void Stop()
    {
        lock (_lockObject)
        {
            _internalCts?.Cancel();
        }

        if (_myThread != null && _myThread.IsAlive)
        {
            if (!_myThread.Join(1000))
            {
                Console.WriteLine($"Warning: Philosopher {Name} didn't stop gracefully");
            }
        }
        
        _myThread = null;
        _internalCts?.Dispose();
        _internalCts = null;
    }

    private void Run(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                ProcessState();
            }
        }
        catch (OperationCanceledException)
        {
            throw new ApplicationException("Operation canceled");
        }
        finally
        {
            // Освобождаем вилки при завершении
            LeftFork?.Put();
            RightFork?.Put();
        }
    }

    private void ProcessState()
    {
        switch (_state)
        {
            case PhilosopherStates.Thinking:
                ProcessThinkingState();
                break;
            case PhilosopherStates.Hungry:
                ProcessHungryState();
                break;
            case PhilosopherStates.TakeLeftFork:
                ProcessTakingLeftForkState();
                break;
            case PhilosopherStates.TakeRightFork:
                ProcessTakingRightForkState();
                break;
            case PhilosopherStates.Eating:
                ProcessEatingState();
                break;
        }
    }

    private void ProcessThinkingState()
    {
        while (_stateTimer < _thinkingTime)
        {
            //Console.WriteLine("Th {0}, {1}", _stateTimer, _eatingTime);
            Thread.Sleep(_thinkingTime / 8);
            Interlocked.Add(ref _stateTimer, _thinkingTime / 8);
        }

        lock (_lockObject)
        {
            _stateTimer = 0;
            _state = PhilosopherStates.Hungry;
        }
    }

    private void ProcessHungryState()
    {
        Thread.Sleep(_takeForkTime);

        lock (_lockObject)
        {
            HungryTime += _takeForkTime;
        }
        Interlocked.Add(ref _stateTimer, _takeForkTime);

        if (FirstTakeLeftFork)
        {
            if (TryTakeLeftFork())
            {
                lock (_lockObject)
                {
                    _state = PhilosopherStates.TakeLeftFork;
                    _stateTimer = 0;
                }
            }
        }
        else
        {
            if (TryTakeRightFork())
            {
                lock (_lockObject)
                {
                    _state = PhilosopherStates.TakeRightFork;
                    _stateTimer = 0;
                }
            }
        }
    }

    private void ProcessTakingLeftForkState()
    {
        Thread.Sleep(_takeForkTime);

        Interlocked.Add(ref _stateTimer, _takeForkTime);

        if (TryTakeRightFork())
        {
            lock (_lockObject)
            {
                _state = PhilosopherStates.Eating;
                _stateTimer = 0;
            }
        }
    }

    private void ProcessTakingRightForkState()
    {
        Thread.Sleep(_takeForkTime);

        Interlocked.Add(ref _stateTimer, _takeForkTime);

        if (TryTakeLeftFork())
        {
            lock (_lockObject)
            {
                _state = PhilosopherStates.Eating;
                _stateTimer = 0;
            }
        }
    }

    private void ProcessEatingState()
    {
        while (_stateTimer < _eatingTime)
        {
            Thread.Sleep(_eatingTime / 8);
            Interlocked.Add(ref _stateTimer, _eatingTime / 8);
        }

        LeftFork?.Put();
        RightFork?.Put();
        
        lock (_lockObject)
        {
            CountEatingFood++;

            _state = PhilosopherStates.Thinking;
            _stateTimer = 0;
        }
    }

    private bool TryTakeLeftFork()
    {
        return LeftFork?.TryTake(this) == true;
    }

    private bool TryTakeRightFork()
    {
        return RightFork?.TryTake(this) == true;
    }

    public void PrintInfo()
    {
        lock (_lockObject)
        {
            string stateInfo = _state switch
            {
                PhilosopherStates.Thinking => $"Thinking ({_stateTimer} ms)",
                PhilosopherStates.Hungry => $"Hungry ({_stateTimer} ms)",
                PhilosopherStates.Eating => $"Eating ({_stateTimer} ms)",
                PhilosopherStates.TakeLeftFork => $"Taking Left Fork ({_stateTimer} ms)",
                PhilosopherStates.TakeRightFork => $"Taking Right Fork ({_stateTimer} ms)",
                _ => _state.ToString()
            };
            
            Console.WriteLine($"{Name}: {stateInfo}, meals: {CountEatingFood}");
        }
    }

    public void PrintScore(double simulationTime)
    {
        lock (_lockObject)
        {
            double throughput = simulationTime > 0 ? CountEatingFood / simulationTime : 0;
            double hungryPercentage = simulationTime > 0 ? (HungryTime / simulationTime) * 100 : 0;
            Console.WriteLine($"{Name}: throughput {throughput:F4} meals/ms, " +
                            $"hungry {HungryTime} ms ({hungryPercentage:F1}%)");
        }
    }

    public bool IsEating()
    {
        lock (_lockObject)
        {
            return _state == PhilosopherStates.Eating;
        }
    }
}

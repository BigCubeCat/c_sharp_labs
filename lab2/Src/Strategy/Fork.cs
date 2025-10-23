using System;
using System.Diagnostics;
using System.Threading;
using Interface;

namespace Src.Strategy;

public class Fork : IForkStrategy
{
    private bool _isTaken;
    private readonly object _lockObject = new object();
    private readonly int _number;
    private Stopwatch _stopwatch = new ();
    
    public IPhilosopher? Owner { get; private set; }
    public long UsedTime { get; private set; }
    public long AvailableTime { get; private set; }
    public long BlockTime { get; private set; }

    public static IFork Create(int number)
    {
        return new Fork(number);
    }

    public Fork(int number)
    {
        _number = number;
        UsedTime = 0; //ms
        BlockTime = 0; //ms
        AvailableTime = 0; //ms

        _stopwatch.Start();
    }

    public bool TryTake(IPhilosopher philosopher)
    {
        lock (_lockObject)
        {
            if (_isTaken)
                return false;

            _stopwatch.Stop();
            AvailableTime += _stopwatch.ElapsedMilliseconds;

            _isTaken = true;
            Owner = philosopher;

            _stopwatch = Stopwatch.StartNew();
            return true;
        }
    }

    public void Put()
    {
        lock (_lockObject)
        {
            _stopwatch.Stop();
            UsedTime += _stopwatch.ElapsedMilliseconds;

            _isTaken = false;
            Owner = null;

            _stopwatch = Stopwatch.StartNew();
        }
    }

    public void PrintInfo()
    {
        lock (_lockObject)
        {
            string status = _isTaken ? $"In Use (by {Owner?.Name})" : "Available";
            Console.WriteLine($"Fork-{_number}: {status}");
        }
    }

    public void PrintScore(double simulationTime)
    {
        lock (_lockObject)
        {
            double usedPercent = simulationTime > 0 ? (UsedTime / simulationTime) * 100 : 0;
            double availablePercent = simulationTime > 0 ? (AvailableTime / simulationTime) * 100 : 0;
            double blockPercent = simulationTime > 0 ? (BlockTime / simulationTime) * 100 : 0;
            Console.WriteLine($"Fork-{_number}: used {usedPercent:F1}%, available {availablePercent:F1}%, blocked {blockPercent:F1}%");
        }
    }
}
global using Interface.Strategy;

using System;
using System.Diagnostics;
using System.Threading;
using Interface;
using Interface.Strategy;
using Src;

public class Simulation
{
    private int _simulationTime;
    private int _updateTime;
    private string _pathToConf;
    private readonly Stopwatch _stopwatch = new();
    private CancellationTokenSource? _cancellationTokenSource;

    public void Run()
    {
        try
        {
            ParseArgs(out _pathToConf, out bool helpOnly, out _updateTime, out _simulationTime);
            if (helpOnly) return;

            Loader.LoadPhilosophersFromFile<Src.Strategy.Philosopher, Src.Strategy.Fork>(_pathToConf, new Random());

            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            _stopwatch.Start();
            MainLoop(_updateTime, _simulationTime, token);
            _stopwatch.Stop();
        }
        catch (ApplicationException e)
        {
            Console.WriteLine(e.Message);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Simulation was cancelled.");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.Write(e.StackTrace);
        }
        finally
        {
            _cancellationTokenSource?.Cancel();
            Thread.Sleep(100);
            PrintFinalStats(_stopwatch.ElapsedMilliseconds);
        }
    }

    private void MainLoop(int updateTime, int simulationTime, CancellationToken token)
    {
        var philosophers = Loader.philosophers;
        var forks = Loader.forks;

        foreach (var philosopher in philosophers)
        {
            philosopher.Start(token);
        }

        var loopWatch = Stopwatch.StartNew();
        long lastUpdate = 0;
        bool isDeadlock = false;

        try
        {
            while (loopWatch.ElapsedMilliseconds < simulationTime && !token.IsCancellationRequested)
            {
                long current = loopWatch.ElapsedMilliseconds;

                if (current - lastUpdate >= updateTime)
                {
                    if (DeadlockAnalyzer.IsDeadlock(philosophers, forks))
                        isDeadlock = true;

                    Console.Clear();
                    Console.WriteLine($"======== TIME: {current} ms ========");
                    Console.WriteLine("Philosophers:");
                    foreach (var philosopher in philosophers)
                        philosopher.PrintInfo();

                    Console.WriteLine("\nForks:");
                    foreach (var fork in forks)
                        fork.PrintInfo();

                    lastUpdate = current;

                    if (isDeadlock)
                    {
                        Console.WriteLine("\nDEADLOCK DETECTED");
                        return;
                    }
                }
                Thread.Sleep(updateTime);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Main loop cancelled.");
        }

        foreach (var philosopher in philosophers)
            philosopher.Stop();

        Console.WriteLine("\nSimulation completed!");
    }

    private void PrintFinalStats(double totalTime)
    {
        var philosophers = Loader.philosophers;
        var forks = Loader.forks;

        int totalMeals = 0;
        int totalHungryTime = 0;
        double maxHungry = 0;
        string mostHungry = "";

        Console.WriteLine("======== FINAL STATISTICS ========");
        Console.WriteLine($"Total simulation time: {totalTime:F2} ms");
        Console.WriteLine("\nPhilosophers:");

        foreach (var philosopher in philosophers)
        {
            philosopher.PrintScore(totalTime);
            totalMeals += philosopher.CountEatingFood;
            totalHungryTime += philosopher.HungryTime;

            if (philosopher.HungryTime > maxHungry)
            {
                maxHungry = philosopher.HungryTime;
                mostHungry = philosopher.Name;
            }
        }

        double throughput = totalTime > 0 ? totalMeals / totalTime : 0;
        Console.WriteLine($"\nThroughput: {throughput:F4} meals/ms");

        double avgWait = philosophers.Count > 0 ? (double)totalHungryTime / philosophers.Count : 0;
        Console.WriteLine($"Average waiting time: {avgWait:F2} ms");
        Console.WriteLine($"Max waiting time: {maxHungry:F2} ms ({mostHungry})");

        Console.WriteLine("\nForks utilization:");
        foreach (var fork in forks)
            fork.PrintScore(totalTime);
    }

    private void ParseArgs(out string path, out bool helpOnly, out int updateTime, out int simulationTime)
    {
        path = "./philosophers.conf";
        updateTime = 150;
        simulationTime = 10000;
        helpOnly = false;

        bool wasConf = false, wasUpd = false, wasSim = false;
        bool confFlag = false, updFlag = false, simFlag = false;

        var args = Environment.GetCommandLineArgs();
        foreach (var arg in args)
        {
            if (confFlag)
            {
                if (wasConf) throw new ArgumentException("Double set path");
                path = arg; wasConf = true; confFlag = false;
            }
            else if (updFlag)
            {
                if (wasUpd) throw new ArgumentException("Double set update time");
                if (!int.TryParse(arg, out updateTime))
                    throw new ArgumentException("Update time should be int");
                wasUpd = true; updFlag = false;
            }
            else if (simFlag)
            {
                if (wasSim) throw new ArgumentException("Double set simulation time");
                if (!int.TryParse(arg, out simulationTime))
                    throw new ArgumentException("Simulation time should be int");
                wasSim = true; simFlag = false;
            }

            if (arg is "-c" or "--config_path") confFlag = true;
            else if (arg is "-t" or "--update_time") updFlag = true;
            else if (arg is "-s" or "--simulation_time") simFlag = true;
            else if (arg is "-h" or "--help")
            {
                Console.Write(
                    """
                    This is lab1 of NSU C# course.

                    *DESCRIPTION*
                    Dining Philosophers simulation.

                    *ARGUMENTS*
                    -c or --config_path       Path to config file.
                    -t or --update_time       State update time (100–200 ms)
                    -s or --simulation_time   Simulation duration in ms
                    -h or --help              Show help
                    """
                );
                helpOnly = true;
            }
        }

        if (updateTime is < 100 or > 200)
        {
            Console.WriteLine("Warning: Update time must be 100–200 ms. Using default 150 ms");
            updateTime = 150;
        }
    }
}

// Запуск
class Program
{
    static void Main()
    {
        new Simulation().Run();
    }
}

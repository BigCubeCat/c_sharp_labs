global using Interface.Strategy;

using System;
using System.Diagnostics;
using System.Threading;
using Interface;
using Src;

int simulationTime = 0;
var stopwatch = new Stopwatch();
CancellationTokenSource? cancellationTokenSource = null;

try
{
    ParseArgs(out string pathToConf, out bool helpOnly, out int updateTime, out simulationTime);

    if (helpOnly) return;

    Loader.LoadPhilosophersFromFile<Src.Strategy.Philosopher, Src.Strategy.Fork>(pathToConf, new Random());
    
    cancellationTokenSource = new CancellationTokenSource();
    var cancellationToken = cancellationTokenSource.Token;
    
    stopwatch.Start();
    MainLoop(updateTime, simulationTime, cancellationToken);
    stopwatch.Stop();
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
    cancellationTokenSource?.Cancel();
    
    // Даем время потокам завершиться
    Thread.Sleep(100);
    
    PrintFinalStats(stopwatch.ElapsedMilliseconds);
}

void MainLoop(int updateTime, int simulationTime, CancellationToken cancellationToken)
{
    var philosophers = Loader.philosophers;
    var forks = Loader.forks;

    // Запускаем все потоки философов с передачей CancellationToken
    foreach (var philosopher in philosophers)
    {
        philosopher.Start(cancellationToken);
    }

    var stopwatch = Stopwatch.StartNew();
    long lastUpdateTime = 0;
    bool isDeadlock = false;

    try
    {
        while (stopwatch.ElapsedMilliseconds < simulationTime && !cancellationToken.IsCancellationRequested)
        {
            long currentTime = stopwatch.ElapsedMilliseconds;
            
            // Обновляем состояние каждые updateTime мс
            if (currentTime - lastUpdateTime >= updateTime)
            {
                if (DeadlockAnalyzer.IsDeadlock(philosophers, forks))
                {
                    isDeadlock = true;
                }

                Console.Clear();
                Console.WriteLine($"======== TIME: {currentTime} ms ========");
                Console.WriteLine("Philosophers:");

                foreach (var philosopher in philosophers)
                {
                    philosopher.PrintInfo();
                }

                Console.WriteLine("\nForks:");

                foreach (var fork in forks)
                {
                    fork.PrintInfo();
                }

                lastUpdateTime = currentTime;

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

    // Останавливаем философов
    foreach (var philosopher in philosophers)
    {
        philosopher.Stop();
    }

    Console.WriteLine("\nSimulation completed!");
}

void PrintFinalStats(double totalSimulationTime)
{
    var philosophers = Loader.philosophers;
    var forks = Loader.forks;

    int allEatingFood = 0;
    int allHungryTime = 0;
    double maxHungryTime = 0;
    string mostHungryPhilosopher = "";

    Console.WriteLine("======== FINAL STATISTICS ========");
    Console.WriteLine($"Total simulation time: {totalSimulationTime:F2} ms");
    Console.WriteLine("\nPhilosophers:");

    foreach (var philosopher in philosophers)
    {
        philosopher.PrintScore(totalSimulationTime);
        allEatingFood += philosopher.CountEatingFood;
        allHungryTime += philosopher.HungryTime;

        if (philosopher.HungryTime > maxHungryTime)
        {
            maxHungryTime = philosopher.HungryTime;
            mostHungryPhilosopher = philosopher.Name;
        }
    }

    // Пропускная способность (еда/миллисекунда)
    double throughput = totalSimulationTime > 0 ? allEatingFood / totalSimulationTime : 0;
    Console.WriteLine($"\nThroughput: {throughput:F4} meals/ms");

    // Среднее время ожидания
    double avgWaitingTime = philosophers.Count > 0 ? (double)allHungryTime / philosophers.Count : 0;
    Console.WriteLine($"Average waiting time: {avgWaitingTime:F2} ms");
    Console.WriteLine($"Max waiting time: {maxHungryTime:F2} ms ({mostHungryPhilosopher})");

    Console.WriteLine("\nForks utilization:");

    foreach (var fork in forks)
    {
        fork.PrintScore(totalSimulationTime);
    }
}

void ParseArgs(out string pathToConf, out bool helpOnly, out int updateTime, out int simulationTime)
{
    pathToConf = "./philosophers.conf";
    updateTime = 150; // 150 мс по умолчанию
    simulationTime = 10000; // 10 секунд по умолчанию
    helpOnly = false;

    bool wasConfigPath = false;
    bool wasUpdateTime = false;
    bool wasSimulationTime = false;

    bool confPathFlag = false;
    bool updTimeFlag = false;
    bool simulationTimeFlag = false;

    var args = Environment.GetCommandLineArgs();

    foreach (var arg in args)
    {
        if (confPathFlag)
        {
            if (wasConfigPath)
                throw new ArgumentException("Double set path");

            pathToConf = arg;
            wasConfigPath = true;
            confPathFlag = false;
        }
        else if (updTimeFlag)
        {
            if (wasUpdateTime)
                throw new ArgumentException("Double set update time");

            if (!int.TryParse(arg, out updateTime))
                throw new ArgumentException("Update time should be int");

            wasUpdateTime = true;
            updTimeFlag = false;
        }
        else if (simulationTimeFlag)
        {
            if (wasSimulationTime)
                throw new ArgumentException("Double set simulation time");

            if (!int.TryParse(arg, out simulationTime))
                throw new ArgumentException("Simulation time should be int");

            wasSimulationTime = true;
            simulationTimeFlag = false;
        }

        if (arg == "-c" || arg == "--config_path")
            confPathFlag = true;
        else if (arg == "-t" || arg == "--update_time")
            updTimeFlag = true;
        else if (arg == "-s" || arg == "--simulation_time")
            simulationTimeFlag = true;
        else if (arg == "-h" || arg == "--help")
        {
            Console.Write(
                """
                This is lab1 of NSU C# course.

                *DESCRIPTION*
                In this lab, I solved the Dining Philosophers problem using multiple threads.

                *ARGUMENTS*
                -c or --config_path - relative or full path to config file. Current directory used by default.
                -h or --help - see this page
                -t or --update_time - time between updates of the simulation state output (100-200 ms)
                -s or --simulation_time - time during which the simulation will run in milliseconds
                """
            );

            helpOnly = true;
        }
    }

    // Проверяем, что updateTime в диапазоне 100-200 мс
    if (updateTime < 100 || updateTime > 200)
    {
        Console.WriteLine($"Warning: Update time should be between 100-200 ms. Using default: 150 ms");
        updateTime = 150;
    }
}
global using Interface.Strategy;
using System;
using Interface;
using Src;

int countIterations = 0;

try
{
    ParseArgs(out ProgramMode progMode, out string pathToConf, out bool helpOnly, out int updateTime,
        out countIterations);

    if (helpOnly) return;

    switch (progMode)
    {
        case ProgramMode.StrategyMode:
        {
            Loader.LoadPhilosophersFromFile<Src.Strategy.Philosopher, Src.Strategy.Fork>(pathToConf, new Random());
            MainLoop(updateTime, countIterations);
            break;
        }
        case ProgramMode.StrategyDeadlockMode:
        {
            Loader.LoadPhilosophersFromFile<Src.Strategy.Philosopher, Src.Strategy.Fork>(pathToConf, new Random(),
                true);
            MainLoop(updateTime, countIterations);
            break;
        }
        case ProgramMode.ControllerMode:
        {
            Loader.LoadPhilosophersFromFile<Src.Controller.Philosopher, Src.Controller.Fork, Src.Controller.Waiter>(
                pathToConf, new Random());
            MainLoop(updateTime, countIterations, true);
            break;
        }
        case ProgramMode.ControllerDeadlockMode:
        {
            Loader.LoadPhilosophersFromFile<Src.Controller.Philosopher, Src.Controller.Fork, Src.Controller.Waiter>(
                pathToConf, new Random(), true);
            MainLoop(updateTime, countIterations, true);
            break;
        }
        default: throw new NotImplementedException("This mode not supported");
    }
}
catch (ApplicationException e)
{
    Console.WriteLine(e.Message);
}
catch (NotImplementedException e)
{
    Console.WriteLine(e.Message);
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
    Console.Write(e.StackTrace);
}
finally
{
    var philosophers = Loader.philosophers;
    var forks = Loader.forks;

    int allEatingFood = 0;
    double time = countIterations / 1000.0d;

    int allHungryTime = 0;
    int maxHungryTime = 0;
    string mostHungryPhilosopher = "";

    Console.WriteLine("======== SCORES ========");
    Console.WriteLine("Philosophers:");

    foreach (var philosopher in philosophers)
    {
        philosopher.PrintScore(time);
        allEatingFood += philosopher.CountEatingFood;
        allHungryTime += philosopher.HungryTime;

        if (philosopher.HungryTime > maxHungryTime)
        {
            maxHungryTime = philosopher.HungryTime;
            mostHungryPhilosopher = philosopher.Name;
        }
    }

    Console.WriteLine("\nMiddle bandwidth: {0} (eat / (1000 * steps)",
        ((double)allEatingFood) / philosophers.Count / time);

    Console.WriteLine("\nMiddle hungry time: {0} (steps), max hungry time: {1} on {2}",
        ((double)allHungryTime) / philosophers.Count, maxHungryTime, mostHungryPhilosopher);

    Console.WriteLine("\nForks:");

    foreach (var fork in forks)
    {
        fork.PrintScore(countIterations);
    }
}

void MainLoop(int updateTime, int countIterations, bool useWaiter = false)
{
    var philosophers = Loader.philosophers;
    var forks = Loader.forks;

    bool isDeadlock = false;
    for (int i = 0; i < countIterations; ++i)
    {
        if (DeadlockAnalyzer.IsDeadlock(philosophers, forks))
        {
            isDeadlock = true;
        }

        Console.WriteLine("======== STEP {0} ========", i);
        Console.WriteLine("Philosophers:");

        foreach (var philosopher in philosophers)
        {
            philosopher.PrintInfo();
        }

        Console.WriteLine("\nForks:");

        foreach (var fork in forks)
        {
            fork.PrintInfo();
            fork.Step();
        }

        foreach (var philosopher in philosophers)
        {
            philosopher.Step();
        }

        if (useWaiter) Loader.waiter!.Step();

        if (isDeadlock)
        {
            Console.WriteLine("\nDEADLOCK DETECTED");
            return;
        }

        Thread.Sleep(updateTime);
        Console.Clear();
    }
}

void ParseArgs(out ProgramMode progMode, out string pathToConf, out bool helpOnly, out int updateTime,
    out int countIterations)
{
    progMode = ProgramMode.StrategyMode;
    pathToConf = "./philosophers.conf";
    updateTime = 1000;
    countIterations = 1000000;
    helpOnly = false;

    bool wasMode = false;
    bool wasConfigPath = false;
    bool wasUpdateTime = false;
    bool wasCountIterations = false;

    bool modeFlag = false;
    bool confPathFlag = false;
    bool updTimeFlag = false;
    bool countIterationsFlag = false;

    foreach (var arg in args)
    {
        if (modeFlag)
        {
            if (wasMode)
                throw new ArgumentException("Double set mode");

            progMode = ProgramModeExtension.ToMode(arg);
            wasMode = true;
            modeFlag = false;
        }
        else if (confPathFlag)
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
        else if (countIterationsFlag)
        {
            if (wasCountIterations)
                throw new ArgumentException("Double set count iterations");

            if (!int.TryParse(arg, out countIterations))
                throw new ArgumentException("Count iterations should be int");

            wasCountIterations = true;
            countIterationsFlag = false;
        }

        if (arg == "-m" || arg == "--mode")
            modeFlag = true;
        else if (arg == "-c" || arg == "--config_path")
            confPathFlag = true;
        else if (arg == "-t" || arg == "--update_time")
            updTimeFlag = true;
        else if (arg == "-i" || arg == "--count_iterations")
            countIterationsFlag = true;
        else if (arg == "-h" || arg == "--help")
        {
            Console.Write
            (
                """
                    This is lab1 of NSU C# course.

                    *DESCRIPTION*
                    In this lab, I solved the Dining Philosophers problem in a single thread. Several solution modes
                    are presented: without controller, without controller with deadlock, with controller and without controller with deadlock. 
                    The default mode is without controller.

                    *ARGUMENTS*
                    -m or --mode - set execution mode. Correct values: strategy, strategy_deadlock, controller, controller_deadlock.
                        Default value: strategy.
                    -c or --config_path - relative or full path to config file. Current directory used by default.
                    -h or --help - see this page
                    -t or --update_time - time between steps of simulation (simulation steps period)
                    -i or --count_iterations - count steps of simulation
                """
            );

            helpOnly = true;
        }
    }
}
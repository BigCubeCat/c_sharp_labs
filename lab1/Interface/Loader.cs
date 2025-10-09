using System.Runtime.CompilerServices;
using Interface.Controller;
using Interface.Strategy;

namespace Interface;

/// <summary>
/// Load all philosophers from file
/// </summary>
public static class Loader
{
    public static readonly List<IPhilosopher> philosophers = [];
    public static readonly List<IFork> forks = [];
    public static IWaiter? waiter = null;

    /// <summary>
    ///     Load philosophers from file
    /// </summary>
    /// <param name="filePath">Path to file with philosophers</param>
    /// <param name="random">Some realization of random number generator</param>
    /// <param name="deadlockConfigure">Configure philosophers for deadlock</param>
    /// <typeparam name="T">Defines a concrete implementation of the IPhilosopher interface</typeparam>
    /// <typeparam name="U">Defines a concrete implementation of the IFork interface</typeparam>
    /// <throws>ArgumentException</throws>
    public static void LoadPhilosophersFromFile<T, U>(string filePath, Random random, bool deadlockConfigure = false)
        where T : class, IPhilosopherStrategy
        where U : class, IForkStrategy
    {
        CreatePhilosophersAndForks<T, U>(filePath, random);
        for (int i = 0; i < forks.Count; ++i)
        {
            ((T)philosophers[i]).RightFork = (IForkStrategy)forks[i];
            ((T)philosophers[i]).LeftFork = (IForkStrategy)forks[(i + 1) % forks.Count];
            ((T)philosophers[i]).FirstTakeLeftFork = false;
        }

        if (!deadlockConfigure) ((T)philosophers[^1]).FirstTakeLeftFork = true;
    }

    /// <summary>
    ///     Load philosophers from file
    /// </summary>
    /// <param name="filePath">Path to file with philosophers</param>
    /// <param name="random">Some realization of random number generator</param>
    /// <param name="deadlockConfigure">Configure philosophers for deadlock</param>
    /// <typeparam name="T">Defines a concrete implementation of the IPhilosopher interface</typeparam>
    /// <typeparam name="U">Defines a concrete implementation of the IFork interface</typeparam>
    /// <throws>ArgumentException</throws>
    public static void LoadPhilosophersFromFile<T, U, C>(string filePath, Random random, bool deadlockConfigure = false)
        where T : class, IPhilosopherController
        where U : class, IForkController
        where C : class, IWaiter
    {
        CreatePhilosophersAndForks<T, U>(filePath, random);

        List<IPhilosopherController> philosophersMain = [];
        List<IForkController> forksMain = [];

        for (int i = 0; i < philosophers.Count; ++i)
        {
            philosophersMain.Add((T)philosophers[i]);
            forksMain.Add((U)forks[i]);
        }
        
        waiter = C.Create(philosophersMain, forksMain, deadlockConfigure);
    }

    private static void CreatePhilosophersAndForks<T, U>(string filePath, Random random)
        where T : class, IPhilosopher
        where U : class, IFork
    {
        using var reader = new StreamReader(filePath);
        if (reader.Peek() < 0)
            throw new ArgumentException("Configuration file is empty");

        string? line = null;

        // move this ranges to config file
        var thinkingRange = new int[] { 3, 10 };
        var eatingRange = new int[] { 4, 5 };

        int takeForkTime = 2;
        int putForkTimeout = 3;
        int eatingTime;
        int thinkingTime;

        int num = 1;
        do
        {
            line = reader.ReadLine();

            // add some validation here

            eatingTime = random.Next(eatingRange[0], eatingRange[1]);
            thinkingTime = random.Next(thinkingRange[0], thinkingRange[1]);

            if (line is not null)
            {
                philosophers.Add(T.Create(
                    new PhilosopherDTO()
                    {
                        Name = line,
                        EatingTime = eatingTime,
                        TakeForkTime = takeForkTime,
                        ThinkingTime = thinkingTime,
                        PutForkTimeout = putForkTimeout
                    }
                ));
                forks.Add(U.Create(num));
            }

            ++num;
        }
        while (line is not null);
    }
}

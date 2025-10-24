using Xunit;
using System.Threading;
using System.Linq;
using Src;               // Namespace Simulation
using Interface;         // Для Loader, IPhilosopher, IFork
using Interface.Strategy;

public class DeadlockTests
{
    [Fact]
    public void Simulation_Should_Detect_Deadlock()
    {
        // ARRANGE
        // Подготовим config-файл с минимальным числом философов (например 5)
        // Файл philosophers.conf уже есть в корне — Simulation сам его прочитает.
        var simulation = new Simulation();

        // ACT
        // Запускаем симуляцию в отдельном потоке, чтобы тест не блокировался.
        var simThread = new Thread(simulation.Run);
        simThread.Start();

        // Ждём, чтобы философы перешли в состояние Hungry и могли "застрять"
        Thread.Sleep(3000);

        // Получаем философов из Loader (а не из Simulation)
        var philosophers = Loader.philosophers;

        // ASSERT
        // Условный deadlock — все философы голодные и никто не ест.
        // Можно сделать тест мягким: хотя бы 3-5 философов в состоянии Hungry.
        Assert.NotNull(philosophers);
        Assert.NotEmpty(philosophers);

        // Никто не ест
        bool allHungry = philosophers.All(p => !p.IsEating());
        Assert.True(allHungry, "Ожидалось, что все философы не едят.");

        // Завершаем поток симуляции, если он ещё работает.
        // (Simulation.Run сам завершится, если детектирует deadlock)
        if (simThread.IsAlive)
            simThread.Interrupt();
    }
}

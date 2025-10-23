namespace Interface.Strategy;

public interface IForkStrategy : IFork
{
    bool TryTake(IPhilosopher philosopher);
}

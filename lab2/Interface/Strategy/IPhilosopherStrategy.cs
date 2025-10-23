namespace Interface.Strategy;

public interface IPhilosopherStrategy : IPhilosopher
{
    IForkStrategy? LeftFork { get; protected internal set; }
    IForkStrategy? RightFork { get; protected internal set; }
    bool FirstTakeLeftFork { get; protected internal set; }
}

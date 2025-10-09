namespace Interface.Strategy;

/// <summary>
/// Strategy-mode philosopher extension. Adds left/right fork references and first-fork preference.
/// </summary>
public interface IPhilosopherStrategy : IPhilosopher
{
    /// <summary>
    /// Left fork reference (may be null until loader wires objects).
    /// </summary>
    IForkStrategy? LeftFork { get; protected internal set; }
    /// <summary>
    ///     Right fork reference.
    /// </summary>
    IForkStrategy? RightFork { get; protected internal set; }
    
    /// <summary>
    ///     When true philosopher first attempts to take left fork, otherwise attempts right fork first.
    ///     Used to break symmetry and avoid deadlock in strategy mode.
    /// </summary>
    bool FirstTakeLeftFork { get; protected internal set; }
}

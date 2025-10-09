namespace Src;

/// <summary>
/// Actions used by philosopher finite-state machines to describe current intent.
/// </summary>
public enum Actions
{
    ReleaseForks,
    TakeRightFork,
    TakeLeftFork,
    TryTakeRightFork,
    TryTakeLeftFork,
    TryTakeFork,
    None
}


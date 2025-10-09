namespace Src;

/// <summary>
/// Execution modes supported by the program.
/// </summary>
public enum ProgramMode
{
    StrategyMode,
    StrategyDeadlockMode,
    ControllerMode,
    ControllerDeadlockMode
}

/// <summary>
/// Helper extensions for parsing mode names.
/// </summary>
public static class ProgramModeExtension
{
    public static ProgramMode ToMode(string stringMode)
    {
        return stringMode switch
        {
            "strategy" => ProgramMode.StrategyMode,
            "strategy_deadlock" => ProgramMode.StrategyDeadlockMode,
            "controller" => ProgramMode.ControllerMode,
            "controller_deadlock" => ProgramMode.ControllerDeadlockMode,
            _ => throw new NotImplementedException("Invalid program mode " + stringMode),
        };
    }
}


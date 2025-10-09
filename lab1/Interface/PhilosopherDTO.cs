namespace Interface;

/// <summary>
/// Data transfer object used to pass philosopher configuration to factory methods.
/// </summary>
sealed public class PhilosopherDTO
{
    /// <summary>
    /// Amount of simulation steps required to eat a portion.
    /// </summary>
    public int EatingTime { get; set; }
    /// <summary>
    ///     Amount of simulation steps needed to take a fork.
    /// </summary>
    public int TakeForkTime { get;  set; }
    
    /// <summary>
    ///     Amount of simulation steps philosopher spends thinking.
    /// </summary>
    public int ThinkingTime { get; set; }
    /// <summary>
    ///     Timeout after which philosopher puts fork back (if implemented).
    /// </summary>
    public int PutForkTimeout { get; set; }
    
    /// <summary>
    ///     Display name for the philosopher (required).
    /// </summary>
    public required string Name { get; set; }
}


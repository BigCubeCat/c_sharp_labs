using Interface;

namespace Philosophers.DB.Entities;

public class Runs
{
    public int RunId { get; set; }
    public int Step { get; set; }
    public long Duration { get; set; }
    public required SimulationStates SimulationState { get; set; }
    public required IList<PhilosophersEntity> Philosophers { get; set; }
}

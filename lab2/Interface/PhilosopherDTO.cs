namespace Interface;

sealed public class PhilosopherDTO
{
    public int EatingTime { get; set; }
    public int TakeForkTime { get;  set; }
    public int ThinkingTime { get; set; }
    public int PutForkTimeout { get; set; }
    public required string Name { get; set; }
}

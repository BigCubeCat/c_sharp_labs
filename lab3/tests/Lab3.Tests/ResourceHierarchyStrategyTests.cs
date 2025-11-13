class SimpleTestPhilosopher : Interface.IPhilosopher
{
    public SimpleTestPhilosopher(Interface.IFork left, Interface.IFork right)
    {
        LeftFork = left;
        RightFork = right;
    }

    public Interface.IFork LeftFork { get; }
    public Interface.IFork RightFork { get; }
    public int CountEatingFood => 0;
    public int HungryTime => 0;
    // <- публичный сеттер
    public string Name { get; set; } = "simple";
    public bool IsEating() => false;
    public string GetInfoString() => "simple";
    public string GetScoreString(double simulationTime) => "simple";
}

class DummyPhilosopher : Interface.IPhilosopher
{
    public Interface.IFork LeftFork => throw new System.NotImplementedException();
    public Interface.IFork RightFork => throw new System.NotImplementedException();
    public int CountEatingFood => 0;
    public int HungryTime => 0;
    // <- публичный сеттер (требование интерфейса)
    public string Name { get; set; } = "dummy";
    public bool IsEating() => false;
    public string GetInfoString() => "dummy";
    public string GetScoreString(double simulationTime) => "dummy";
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Interface;

public interface IFork
{
    long UsedTime { get; }
    long BlockTime { get; }
    long AvailableTime { get; }
    virtual static IFork Create(int number)
    {
        throw new NotImplementedException("Create function not implemented here");
    }
    IPhilosopher? Owner { get; }
    void Put();
    void PrintInfo();
    void PrintScore(double simulationTime);
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Interface;

public interface IForksFactory<T>
    where T: class, IFork
{
    public IFork Create();
}

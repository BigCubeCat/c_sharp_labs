using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Interface;

namespace Philosophers.Services.Utils;

public class ForksFactory<T> : IForksFactory<T>
    where T: class, IFork
{
    private static IFork? _previousFork = null;
    private static int _count = 0;

    public IFork Create()
    {
        var constructor = typeof(T).GetConstructor([typeof(int)]) ?? throw new ApplicationException("Can not find valid constructor");
        if (_previousFork == null)
        {
            ++_count;
            _previousFork = (IFork) constructor.Invoke([_count]);
        }

        var fork = _previousFork;

        ++_count;
        _previousFork = (IFork) constructor.Invoke([_count]);

        return fork;
    }
}

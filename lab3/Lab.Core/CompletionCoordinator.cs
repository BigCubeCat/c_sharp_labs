using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lab.Core;

public class CompletionCoordinator
{
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<CompletionCoordinator> _logger;
    private int _activeServices = 0;

    public CompletionCoordinator(IHostApplicationLifetime lifetime,
        ILogger<CompletionCoordinator> logger)
    {
        _lifetime = lifetime;
        _logger = logger;
    }

    public void RegisterService(string serviceName)
    {
        var count = Interlocked.Increment(ref _activeServices);
    }

    public void CompleteService(string serviceName)
    {
        var count = Interlocked.Decrement(ref _activeServices);
        if (count == 0)
        {
            _logger.LogWarning("All services completed. Stopping host...");
            _lifetime.StopApplication();
        }
    }
}

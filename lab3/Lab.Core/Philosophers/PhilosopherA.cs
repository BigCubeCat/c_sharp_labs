using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Interface;
using Interface.Channel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Lab.Core.Channels.Items;
using Lab.Core.Strategy;

namespace Lab.Core.Philosophers;

public class PhilosopherA : PhilosopherService
{
    public PhilosopherA(
        ILogger<PhilosopherService> logger,
        IStrategy philosopherStrategy,
        IOptions<PhilosopherConfiguration> options,
        IForksFactory<Fork> forksFactory,
        IChannel<PhilosopherToAnalyzerChannelItem> channelToAnalyzer,
        IChannel<PhilosopherToPrinterChannelItem> channelToPrinter)
    : base(logger, philosopherStrategy, options, forksFactory, channelToAnalyzer, channelToPrinter)
    {
        Name = "Кант";
    }
}

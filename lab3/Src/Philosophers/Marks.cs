using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Interface;
using Interface.Channel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Src.Channels.Items;
using Src.Strategy;

namespace Src.Philosophers;

public class Marks : PhilosopherService
{
    public Marks(
        ILogger<PhilosopherService> logger,
        IStrategy philosopherStrategy,
        IOptions<PhilosopherConfiguration> options,
        IForksFactory<Fork> forksFactory,
        IChannel<PhilosopherToAnalyzerChannelItem> channelToAnalyzer,
        IChannel<PhilosopherToPrinterChannelItem> channelToPrinter)
    : base(logger, philosopherStrategy, options, forksFactory, channelToAnalyzer, channelToPrinter)
    {
        Name = "Маркс";
    }
}

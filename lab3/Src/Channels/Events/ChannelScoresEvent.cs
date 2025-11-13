using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Interface.Channel;

namespace Src.Channels.Events;

public class ChannelScoresEvent: IChannelEventArgs
{
    public double SimulationTime { get; set; }
}

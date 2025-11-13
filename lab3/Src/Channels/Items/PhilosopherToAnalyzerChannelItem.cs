using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Interface.Channel;

namespace Src.Channels.Items;

public record PhilosopherToAnalyzerChannelItem(bool IAmEating, bool LeftForkIsFree, bool RightForkIsFree) : IChannelItem;

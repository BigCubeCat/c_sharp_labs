using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Interface.Channel;

namespace Src.Channels.Items;

public record PhilosopherToPrinterChannelItem(string PhilosopherInfo, string LeftForkInfo, string RightForkInfo): IChannelItem;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Interface.Channel;

namespace Lab.Core.Channels.Items;

public record PhilosopherToPrinterChannelItem(string PhilosopherInfo, string LeftForkInfo, string RightForkInfo) : IChannelItem;

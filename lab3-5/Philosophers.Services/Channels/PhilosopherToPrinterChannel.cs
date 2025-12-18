using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using Interface.Channel;
using Philosophers.Services.Channels.Items;

namespace Philosophers.Services.Channels;

public class PhilosopherToPrinterChannel: IChannel<PhilosopherToPrinterChannelItem>
{
    private readonly Channel<PhilosopherToPrinterChannelItem> _channel;

    public ChannelWriter<PhilosopherToPrinterChannelItem> Writer => _channel.Writer;
    public ChannelReader<PhilosopherToPrinterChannelItem> Reader => _channel.Reader;

    public event EventHandler? SendMeItem;
    public event EventHandler<IChannelEventArgs>? SendMeItemBy;
    public event EventHandler? PublisherWantToRegister;

    public PhilosopherToPrinterChannel()
    {
        _channel = Channel.CreateBounded<PhilosopherToPrinterChannelItem>(
            new BoundedChannelOptions(500)
            {
                FullMode = BoundedChannelFullMode.Wait
            }
        );
    }

    public void Notify(object? sender) => SendMeItem?.Invoke(sender, EventArgs.Empty);

    public void NotifyWith(object? sender, IChannelEventArgs args) => SendMeItemBy?.Invoke(sender, args);

    public void RegisterPublisher(object? publisher) => PublisherWantToRegister?.Invoke(publisher, EventArgs.Empty);
}

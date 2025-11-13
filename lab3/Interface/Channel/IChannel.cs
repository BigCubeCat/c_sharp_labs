using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Interface.Channel;

public interface IChannel<T>
    where T: class, IChannelItem
{
    event EventHandler? SendMeItem;
    event EventHandler<IChannelEventArgs>? SendMeItemBy;
    event EventHandler? PublisherWantToRegister;
    ChannelWriter<T> Writer { get; }
    ChannelReader<T> Reader { get; }

    void Notify(object? sender);
    void NotifyWith(object? sender, IChannelEventArgs args);
    void RegisterPublisher(object? publisher);
}

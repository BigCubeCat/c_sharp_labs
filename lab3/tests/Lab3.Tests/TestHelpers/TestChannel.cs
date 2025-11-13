using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using Interface.Channel;

namespace Lab3.Tests.TestHelpers
{
    public class TestChannel<T> : IChannel<T>
        where T : class, IChannelItem
    {
        private readonly Channel<T> _channel;
        public ChannelWriter<T> Writer => _channel.Writer;
        public ChannelReader<T> Reader => _channel.Reader;

        public event EventHandler? SendMeItem;
        public event EventHandler<IChannelEventArgs>? SendMeItemBy;
        public event EventHandler? PublisherWantToRegister;

        public TestChannel(int capacity = 500)
        {
            _channel = Channel.CreateBounded<T>(new BoundedChannelOptions(capacity) { FullMode = BoundedChannelFullMode.Wait });
        }

        public void Notify(object? sender) => SendMeItem?.Invoke(sender, EventArgs.Empty);
        public void NotifyWith(object? sender, IChannelEventArgs args) => SendMeItemBy?.Invoke(sender, args);
        public void RegisterPublisher(object? publisher) => PublisherWantToRegister?.Invoke(publisher, EventArgs.Empty);
    }
}

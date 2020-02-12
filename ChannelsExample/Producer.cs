using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ChannelsExample
{
    public class Producer
    {
        private readonly ChannelWriter<Envelope> _writer;
        private readonly int _instanceId;

        public Producer(ChannelWriter<Envelope> writer, int instanceId)
        {
            _writer = writer;
            _instanceId = instanceId;
        }

        public async Task PublishAsync(Envelope message, CancellationToken cancellationToken = default)
        {
            await _writer.WriteAsync(message, cancellationToken);
            Logger.Log($"Producer {_instanceId} > published '{message.Payload}'", ConsoleColor.Yellow);
        }
    }
}
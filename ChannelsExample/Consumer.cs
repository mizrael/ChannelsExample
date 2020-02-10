using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ChannelsExample
{
    public class Consumer
    {
        private readonly ChannelReader<Envelope> _reader;
        private readonly int _instanceId;
        private static readonly Random Random = new Random();

        public Consumer(ChannelReader<Envelope> reader, int instanceId)
        {
            _reader = reader;
            _instanceId = instanceId;
        }

        public async Task BeginConsumeAsync(CancellationToken cancellationToken = default)
        {
            Logger.Log($"Consumer {_instanceId} > starting", ConsoleColor.Green);
            
            try
            {
                while (await _reader.WaitToReadAsync(cancellationToken))
                {
                    await foreach (var message in _reader.ReadAllAsync(cancellationToken))
                    {
                        Logger.Log($"CONSUMER ({_instanceId})> Received: {message.Payload}", ConsoleColor.Green);
                        await Task.Delay(500, cancellationToken);
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                Logger.Log($"Consumer {_instanceId} > forced stop", ConsoleColor.Green);
            }

            Logger.Log($"Consumer {_instanceId} > shutting down", ConsoleColor.Green);
        }

    }
}
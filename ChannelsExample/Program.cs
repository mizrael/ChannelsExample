using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ChannelsExample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await Run(1, 10, 1, 1);

            await Run(1, 10, 1, 3);

            await Run(100, 10, 1, 3);

            Logger.Log("done!");
            Console.ReadLine();
        }

        private static async Task Run(int maxMessagesToBuffer, int messagesToSend, int producersCount, int consumersCount)
        {
            Logger.Log("*** STARTING EXECUTION ***");
            Logger.Log($"producers #: {producersCount}, buffer size: {maxMessagesToBuffer}, consumers #: {consumersCount}");

            var channel = Channel.CreateBounded<Envelope>(maxMessagesToBuffer);

            var tokenSource = new CancellationTokenSource();
            var cancellationToken = tokenSource.Token;

            var tasks = new List<Task>(StartConsumers(channel, consumersCount, cancellationToken))
            {
                ProduceAsync(channel, messagesToSend, producersCount, tokenSource)
            };
            await Task.WhenAll(tasks);
            Logger.Log("*** EXECUTION COMPLETE ***");
        }

        private static async Task SingleProducerMultiConsumers(int maxMessagesToBuffer, int messagesToSend, int producersCount, int consumersCount)
        {
            var channel = Channel.CreateBounded<Envelope>(maxMessagesToBuffer);

            var tokenSource = new CancellationTokenSource();
            var cancellationToken = tokenSource.Token;

            var tasks = new List<Task>(StartConsumers(channel, consumersCount, cancellationToken))
            {
                ProduceAsync(channel, messagesToSend, producersCount, tokenSource)
            };
            await Task.WhenAll(tasks);
        }

        private static Task[] StartConsumers(Channel<Envelope> channel, int consumersCount, CancellationToken cancellationToken)
        {
            var consumerTasks = Enumerable.Range(1, consumersCount)
                .Select(i => new Consumer(channel.Reader, i).BeginConsumeAsync(cancellationToken))
                .ToArray();
            return consumerTasks;
        }

        private static async Task ProduceAsync(Channel<Envelope> channel, 
            int messagesCount,
            int producersCount,
            CancellationTokenSource tokenSource)
        {
            var producers = Enumerable.Range(1, producersCount)
                .Select(i => new Producer(channel.Writer, i))
                .ToArray();

            int index = 0;

            var tasks = Enumerable.Range(1, messagesCount)
                .Select(i =>
                {
                    index = ++index % producersCount;
                    var producer = producers[index];
                    var msg = new Envelope($"message {i}");
                    return producer.PublishAsync(msg, tokenSource.Token);
                })
                .ToArray();
            await Task.WhenAll(tasks);

            Logger.Log("done publishing, closing writer");
            channel.Writer.Complete();

            Logger.Log("waiting for consumer to complete...");
            await channel.Reader.Completion;

            Logger.Log("Consumers done processing, shutting down...");
            tokenSource.Cancel();
        }
    }
}

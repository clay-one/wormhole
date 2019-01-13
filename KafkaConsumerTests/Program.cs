using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Confluent.Kafka;

namespace KafkaConsumerTests
{
    public class Program
    {
        /// <summary>
        ///     In this example
        ///         - offsets are manually committed.
        ///         - no extra thread is created for the Poll (Consume) loop.
        /// </summary>
        private static void Run_Consume(string brokerList, List<string> topics, CancellationToken cancellationToken)
        {
            var sw = new Stopwatch();
            long count = 0;

            sw.Start();

            Console.WriteLine($"{DateTime.Now:s}_Consumer started");
            var config = new ConsumerConfig
            {
                BootstrapServers = brokerList,
                GroupId = "csharp-consumer",
                EnableAutoCommit = false,
                StatisticsIntervalMs = 5000,
                SessionTimeoutMs = 6000,
                AutoOffsetReset = AutoOffsetResetType.Latest
            };

            const int commitPeriod = 20;

            var timer = new Timer(state =>
            {
                Console.WriteLine($"consumed: {count} message in one second");

            }, null, 0, 1000);

            using (var consumer = new Consumer<Ignore, string>(config))
            {
                // Note: All event handlers are called on the main .Consume thread.

                // Raised when the consumer has been notified of a new assignment set.
                // You can use this event to perform actions such as retrieving offsets
                // from an external source / manually setting start offsets using
                // the Assign method. You can even call Assign with a different set of
                // partitions than those in the assignment. If you do not call Assign
                // in a handler of this event, the consumer will be automatically
                // assigned to the partitions of the assignment set and consumption
                // will start from last committed offsets or in accordance with
                // the auto.offset.reset configuration parameter for partitions where
                // there is no committed offset.
                consumer.OnPartitionsAssigned += (_, partitions)
                    => Console.WriteLine($"Assigned partitions: [{string.Join(", ", partitions)}], member id: {consumer.MemberId}");

                // Raised when the consumer's current assignment set has been revoked.
                consumer.OnPartitionsRevoked += (_, partitions)
                    => Console.WriteLine($"Revoked partitions: [{string.Join(", ", partitions)}]");

                consumer.OnPartitionEOF += (_, tpo)
                    =>
                {
                    Console.WriteLine(
                        $"Reached end of topic {tpo.Topic} partition {tpo.Partition}, next message will be at offset {tpo.Offset}");

                    Console.WriteLine($"{sw.ElapsedMilliseconds}");
                };

                consumer.OnError += (_, e)
                    => Console.WriteLine($"Error: {e.Reason}");

                //consumer.OnStatistics += (_, json)
                //    => Console.WriteLine($"Statistics: {json}");

                consumer.Subscribe(topics);

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = consumer.Consume(cancellationToken);
                        Interlocked.Increment(ref count);

                        if (consumeResult.Offset % commitPeriod == 0)
                        {
                            // The Commit method sends a "commit offsets" request to the Kafka
                            // cluster and synchronously waits for the response. This is very
                            // slow compared to the rate at which the consumer is capable of
                            // consuming messages. A high performance application will typically
                            // commit offsets relatively infrequently and be designed handle
                            // duplicate messages in the event of failure.
                            var committedOffsets = consumer.Commit(consumeResult);
                            //Console.WriteLine($"Committed offset: {committedOffsets}");
                        }
                    }
                    catch (ConsumeException e)
                    {
                        Console.WriteLine($"Consume error: {e.Error}");
                    }
                }


                consumer.Close();
                Console.WriteLine($"consumed: {count} messages in {sw.ElapsedMilliseconds} millis");
            }
        }

        /// <summary>
        ///     In this example
        ///         - consumer group functionality (i.e. .S ubscribe + offset commits) is not used.
        ///         - the consumer is manually assigned to a partition and always starts consumption
        ///           from a specific offset (0).
        /// </summary>
        private static void Run_ManualAssign(string brokerList, List<string> topics, CancellationToken cancellationToken)
        {
            var config = new ConsumerConfig
            {
                // the group.id property must be specified when creating a consumer, even 
                // if you do not intend to use any consumer group functionality.
                GroupId = new Guid().ToString(),
                BootstrapServers = brokerList,
                // partition offsets can be committed to a group even by consumers not
                // subscribed to the group. in this example, auto commit is disabled
                // to prevent this from occuring.
                EnableAutoCommit = true
            };

            using (var consumer = new Consumer<Ignore, string>(config))
            {
                consumer.Assign(topics.Select(topic => new TopicPartitionOffset(topic, 0, Offset.Beginning)).ToList());

                consumer.OnError += (_, e)
                    => Console.WriteLine($"Error: {e.Reason}");

                consumer.OnPartitionEOF += (_, topicPartitionOffset)
                    => Console.WriteLine($"End of partition: {topicPartitionOffset}");

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = consumer.Consume(cancellationToken);
                        Console.WriteLine($"Received message at {consumeResult.TopicPartitionOffset}: ${consumeResult.Message}");
                    }
                    catch (ConsumeException e)
                    {
                        Console.WriteLine($"Consume error: {e.Error}");
                    }
                }

                consumer.Close();
            }
        }

        private static void PrintUsage()
            => Console.WriteLine("Usage: .. <poll|consume|manual> <broker,broker,..> <topic> [topic..]");

        public static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                PrintUsage();
                return;
            }

            var mode = args[0];
            var brokerList = args[1];
            var topics = args.Skip(2).ToList();

            Console.WriteLine($"Started consumer, Ctrl-C to stop consuming");

            CancellationTokenSource cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true; // prevent the process from terminating.
                cts.Cancel();
            };

            switch (mode)
            {
                case "consume":
                    Run_Consume(brokerList, topics, cts.Token);
                    break;
                case "manual":
                    Run_ManualAssign(brokerList, topics, cts.Token);
                    break;
                default:
                    PrintUsage();
                    break;
            }
        }
    }
}

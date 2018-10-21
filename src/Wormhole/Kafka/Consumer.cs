using System;
using System.Threading;
using CommonLogic.Kafka;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Wormhole.Kafka
{
    public abstract class Consumer : IConsumer
    {
        private readonly IKafkaConsumer<Null, string> _consumer;
        protected readonly ILogger Logger;
        protected readonly ConsumerDiagnostic ConsumerDiagnostic;
        private Thread _thread;
        private bool _continue = true;


        protected Consumer(IKafkaConsumer<Null, string> consumer, ILoggerFactory loggerFactory, ConsumerDiagnostic consumerDiagnostic)
        {
            _consumer = consumer;
            _consumer.SetDiagnostic(consumerDiagnostic);
            Logger = loggerFactory.CreateLogger(nameof(Consumer));
            ConsumerDiagnostic = consumerDiagnostic;
        }

        public abstract string Topic { get; }

        public void Dispose()
        {
            _consumer?.Dispose();
        }

        public void Start()
        {
            _consumer.Subscribe(Topic);

            if (_thread == null || !_thread.IsAlive)
            {
                _thread = new Thread(() =>
                {
                    //LogicalThreadContext.Properties["MUID"] = Guid.NewGuid().ToString("N");
                    do
                    {
                        try
                        {
                            _consumer.Poll(TimeSpan.FromMilliseconds(100));
                        }
                        catch (Exception e)
                        {
                            Logger.LogError("exception in thread of consuming", e);
                            ConsumerDiagnostic.IncrementExceptionCount();
                        }
                    } while (_continue);
                });
                _thread.Start();
            }
        }

        public virtual void Stop()
        {
            _continue = false;
        }

    }
}
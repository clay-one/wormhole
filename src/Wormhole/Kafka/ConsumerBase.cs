using System;
using System.Threading;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Wormhole.Kafka
{
    public abstract class ConsumerBase : IConsumerBase
    {
        private readonly IKafkaConsumer<Null, string> _consumer;
        private Thread _thread;
        private bool _continue = true;

        protected readonly ILogger Logger;
        protected readonly ConsumerDiagnostic ConsumerDiagnostic;


        protected ConsumerBase(IKafkaConsumer<Null, string> consumer, ILoggerFactory logger, ConsumerDiagnostic consumerDiagnostic)
        {
            _consumer = consumer;
            _consumer.SetDiagnostic(consumerDiagnostic);
            Logger = logger.CreateLogger(nameof(ConsumerBase));

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
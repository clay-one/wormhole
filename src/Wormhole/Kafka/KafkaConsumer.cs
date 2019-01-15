using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wormhole.Configurations;

namespace Wormhole.Kafka
{
    public class KafkaConsumer: IKafkaConsumer<Null, string>, IDisposable
    {
        private readonly KafkaConfig _configuration;
        private readonly ILogger _logger;
        private readonly ConsumerDiagnostic _consumerDiagnostic;
        private readonly string _topicName;
        private Consumer<Null, string> _consumer;
        private bool _continue = true;
        private Thread _thread;


        public KafkaConsumer(IOptions<KafkaConfig> options, ILoggerFactory logger, ConsumerDiagnostic consumerDiagnostic, string topicName)
        {
            _logger = logger.CreateLogger(nameof(KafkaConsumer));
            _configuration = options.Value;
            _consumerDiagnostic = consumerDiagnostic;
            _topicName = topicName;
			OnError += Error;
			OnConsumeError += ConsumeError;
			OnLog += Log;
        }
        public void Dispose()
        {
            _consumer?.Dispose();
        }

        public event EventHandler<Error> OnError;
        public event EventHandler<Message> OnConsumeError;
        public event EventHandler<LogMessage> OnLog;
        public event EventHandler<Message<Null, string>> OnMessage;
        public string MemberId { get; }
        public List<string> Subscription { get; }

        public void Poll(TimeSpan timeout)
        {
            _consumer.Poll(timeout);
        }

        public void Subscribe(string topic)
        {
            _consumer.Subscribe(topic);
        }

        public void Subscribe(IEnumerable<string> topics)
        {
            _consumer.Subscribe(topics);
        }

        public void Assign(IEnumerable<TopicPartitionOffset> partitions)
        {
            _consumer.Assign(partitions);
        }

        public void Unassign()
        {
            _consumer.Unassign();
        }

        public void Initialize(ICollection<KeyValuePair<string, object>> config, EventHandler<Message<Null, string>> onMessageEventHandler)
        {
            OnMessage = onMessageEventHandler;
            config.Add(new KeyValuePair<string, object>(KafkaConfig.BootstrapServersKey,
                _configuration.ServerAddress));
            config.Add(new KeyValuePair<string, object>(KafkaConfig.ConsumerAutoCommitIntervalMsKey, 
                _configuration.ConsumerAutoCommitIntervalMs));
            config.Add(new KeyValuePair<string, object>(KafkaConfig.EnableAutoCommitKey, _configuration.EnableAutoCommit));
            config.Add(new KeyValuePair<string, object>(KafkaConfig.StatisticsIntervalMsKey, _configuration.StatisticsIntervalMs));
            config.Add(new KeyValuePair<string, object>(KafkaConfig.ConsumerGroupIdKey, _configuration.ConsumerGroupId));
            config.Add(new KeyValuePair<string, object>(KafkaConfig.DefaultTopicConfigKey, new Dictionary<string, object>
            {
                {"auto.offset.reset", "latest"}
            }));

            _consumer = new Consumer<Null, string>(config,null, new StringDeserializer(Encoding.UTF8));

            _consumer.OnMessage += OnMessage;
            _consumer.OnError += OnError;
            _consumer.OnConsumeError += OnConsumeError;
            _consumer.OnLog += OnLog;
            _logger.LogInformation($"consumer : {_consumer.Name} initialized");
        }

        public void Start()
        {
            Subscribe(_topicName);

            if (_thread == null || !_thread.IsAlive)
            {
                _thread = new Thread(() =>
                {
                    //LogicalThreadContext.Properties["MUID"] = Guid.NewGuid().ToString("N");
                    do
                    {
                        try
                        {
                            Poll(TimeSpan.FromMilliseconds(100));
                        }
                        catch (Exception e)
                        {
                            _logger.LogError("exception in thread of consuming", e);
                            _consumerDiagnostic.IncrementExceptionCount();
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

        private void Log(object sender, LogMessage e)
        {
            _logger.LogInformation($"{e.Facility,-12} : {e.Message}");
        }

        private void ConsumeError(object sender, Message message)
        {
            _logger.LogError(
              $"Error consuming from topic/partition/offset {message.Topic}/{message.Partition}/{message.Offset}: {message.Error}");
        }

        private void Error(object sender, Error error1)
        {
            _consumerDiagnostic?.IncrementExceptionCount();
            _logger.LogError($"Error: {error1}");
        }
    }
}
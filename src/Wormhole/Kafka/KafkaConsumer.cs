using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ErrorCode = Confluent.Kafka.ErrorCode;

namespace Wormhole.Kafka
{
    public class KafkaConsumer: IKafkaConsumer<Null, string>
    {
        private readonly ILogger _logger;
        private Consumer<Null, string> _consumer;
        private readonly KafkaConfig _configuration;
        private ConsumerDiagnostic _consumerDiagnostic;

        public KafkaConsumer(IOptions<KafkaConfig> options, ILoggerFactory logger)
        {
            _logger = logger.CreateLogger(nameof(KafkaConsumer));
            _configuration = options.Value;
			OnError += Error;
			OnPartitionsAssigned += PartitionsAssigned;
			OnPartitionEOF += PartitionEof;
			OnStatistics += Statistics;
			OnPartitionsRevoked += PartitionsRevoked;
			OnConsumeError += ConsumeError;
			OnOffsetsCommitted += OffsetsCommitted;
			OnLog += Log;
        }

        private void Log(object sender, LogMessage e)
        {
            _logger.LogInformation($"{e.Facility,-12} : {e.Message}");
        }

        public void Dispose()
        {
            _consumer.Dispose();
        }

        public event EventHandler<Error> OnError;
        public event EventHandler<Message> OnConsumeError;
        public event EventHandler<string> OnStatistics;
        public event EventHandler<LogMessage> OnLog;
        public event EventHandler<CommittedOffsets> OnOffsetsCommitted;
        public event EventHandler<List<TopicPartition>> OnPartitionsRevoked;
        public event EventHandler<List<TopicPartition>> OnPartitionsAssigned;
        public event EventHandler<TopicPartitionOffset> OnPartitionEOF;
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

        public void SetDiagnostic(ConsumerDiagnostic consumerDiagnostic)
        {
            _consumerDiagnostic = consumerDiagnostic;
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
            config.Add(new KeyValuePair<string, object>(KafkaConfig.DefaultTopicConfigKey, new Dictionary<string, object>
            {
                {"auto.offset.reset", "earliest"}
            }));

            _consumer = new Consumer<Null, string>(config,null, new StringDeserializer(Encoding.UTF8));

            _consumer.OnMessage += OnMessage;
            _consumer.OnError += OnError;
            _consumer.OnConsumeError += OnConsumeError;
            _consumer.OnOffsetsCommitted += OnOffsetsCommitted;
            _consumer.OnLog += OnLog;
            _consumer.OnPartitionEOF += OnPartitionEOF;
            _consumer.OnStatistics += OnStatistics;
            _consumer.OnPartitionsRevoked += OnPartitionsRevoked;
            _consumer.OnPartitionsAssigned += OnPartitionsAssigned;
            _logger.LogInformation($"consumer : {_consumer.Name} initialized");

        }

        private void OffsetsCommitted(object sender, CommittedOffsets committedOffsets)
        {
            var commitErrors = string.Join(", ", committedOffsets.Offsets.Where(c => c.Error.Code != ErrorCode.NoError));
            var commitResult = string.IsNullOrWhiteSpace(commitErrors) ? "Success" : commitErrors;
            _logger.LogInformation($"Offsets Commit Status: {commitResult}");

            if (committedOffsets.Error)
                _consumerDiagnostic?.IncrementExceptionCount();
        }

        private void ConsumeError(object sender, Message message)
        {
            _logger.LogError(
              $"Error consuming from topic/partition/offset {message.Topic}/{message.Partition}/{message.Offset}: {message.Error}");
        }

        private void PartitionsRevoked(object sender, List<TopicPartition> topicPartitions)
        {
            _logger.LogInformation($"Revoked partitions: [{string.Join(", ", topicPartitions)}]");
            _consumer.Unassign();
        }

        private void Statistics(object sender, string s)
        {
            _logger.LogInformation($"Statistics: {s}");
        }

        private void PartitionEof(object sender, TopicPartitionOffset topicPartitionOffset)
        {
            _logger.LogInformation(
               $"Reached end of topic {topicPartitionOffset.Topic} partition {topicPartitionOffset.Partition}, next message will be at offset {topicPartitionOffset.Offset}");
        }

        private void PartitionsAssigned(object sender, List<TopicPartition> topicPartitions)
        {
           _logger.LogInformation($"Assigned partitions: [{string.Join(", ", topicPartitions)}], member id: {_consumer.MemberId}");

            _consumer.Assign(topicPartitions);
        }

        private void Error(object sender, Error error1)
        {
            _consumerDiagnostic?.IncrementExceptionCount();
            _logger.LogError($"Error: {error1}");
        }
    }
}
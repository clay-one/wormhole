using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonLogic.Kafka;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using log4net;

namespace Wormhole.Kafka
{
    public class KafkaConsumer: IKafkaConsumer<Null, string>
    {
        private readonly KafkaConfiguration _configuration;
        private readonly ILog _logger;
        private Consumer<Null, string> _consumer;
        private ConsumerDiagnostic _consumerDiagnostic;

        public KafkaConsumer(KafkaConfiguration configuration)
        {
            _configuration = configuration;
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
            config.Add(new KeyValuePair<string, object>(KafkaConfiguration.BootstrapServersKey,
                _configuration.BootstrapServers));
            config.Add(new KeyValuePair<string, object>(KafkaConfiguration.ConsumerAutoCommitIntervalMsKey, 
                _configuration.ConsumerAutoCommitIntervalMs));
            config.Add(new KeyValuePair<string, object>(KafkaConfiguration.EnableAutoCommitKey, _configuration.EnableAutoCommit));
            config.Add(new KeyValuePair<string, object>(KafkaConfiguration.StatisticsIntervalMsKey, _configuration.StatisticsIntervalMs));
            config.Add(new KeyValuePair<string, object>(KafkaConfiguration.DefaultTopicConfigKey, _configuration.DefaultTopicConfig));

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
        }

        private void OffsetsCommitted(object sender, CommittedOffsets committedOffsets)
        {
            var commitErrors = string.Join(", ", committedOffsets.Offsets.Where(c => c.Error.Code != ErrorCode.NoError));
            var commitResult = string.IsNullOrWhiteSpace(commitErrors) ? "Success" : commitErrors;
            _logger.Info($"Offsets Commit Status: {commitResult}");

            if (committedOffsets.Error)
                _consumerDiagnostic?.IncrementExceptionCount();
        }

        private void ConsumeError(object sender, Message message)
        {
            _logger.Error(
                $"Error consuming from topic/partition/offset {message.Topic}/{message.Partition}/{message.Offset}: {message.Error}");
        }

        private void PartitionsRevoked(object sender, List<TopicPartition> topicPartitions)
        {
            _logger.Info($"Revoked partitions: [{string.Join(", ", topicPartitions)}]");
            _consumer.Unassign();
        }

        private void Statistics(object sender, string s)
        {
            _logger.Info($"Statistics: {s}");
        }

        private void PartitionEof(object sender, TopicPartitionOffset topicPartitionOffset)
        {
            _logger.Info(
                $"Reached end of topic {topicPartitionOffset.Topic} partition {topicPartitionOffset.Partition}, next message will be at offset {topicPartitionOffset.Offset}");
        }

        private void PartitionsAssigned(object sender, List<TopicPartition> topicPartitions)
        {
            _logger.Info($"Assigned partitions: [{string.Join(", ", topicPartitions)}], member id: {_consumer.MemberId}");

            _consumer.Assign(topicPartitions);
        }

        private void Error(object sender, Error error1)
        {
            _consumerDiagnostic?.IncrementExceptionCount();
            _logger.Error($"Error: {error1}");
        }
    }
}
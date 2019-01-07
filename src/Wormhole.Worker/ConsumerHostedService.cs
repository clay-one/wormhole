using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wormhole.Configurations;
using Wormhole.DataImplementation;
using Wormhole.DomainModel;
using Wormhole.Kafka;

namespace Wormhole.Worker
{
    public class ConsumerHostedService : IHostedService
    {
        private readonly IKafkaConsumer<Null, string> _kafkaConsumer;
        private readonly ILoggerFactory _loggerFactory;
        private readonly NebulaService _nebulaService;
        private readonly IList<OutputChannel> _outputChannels = new List<OutputChannel>();
        private readonly ITenantDa _tenantDa;

        private readonly IDictionary<string, IConsumerBase> Consumers =
            new ConcurrentDictionary<string, IConsumerBase>();

        public ConsumerHostedService(IOptions<KafkaConfig> options, ITenantDa tenantDa,
            IKafkaConsumer<Null, string> kafkaConsumer, NebulaService nebulaService, ILoggerFactory loggerFactory)
        {
            var kafkaConfig = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _tenantDa = tenantDa;
            _kafkaConsumer = kafkaConsumer;
            _nebulaService = nebulaService;
            _loggerFactory = loggerFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _nebulaService.StartAsync(cancellationToken);
            var topics = await GetTopics();
            StartConsuming(topics);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task<List<string>> GetTopics()
        {
            var topics = await _tenantDa.FindTenants();
            return topics.Select(t => t.Name).ToList();
        }


        private void StartConsuming(List<string> topics)
        {
            if (topics == null || topics.Count < 1) throw new Exception("There is no topic for message consumption");

            foreach (var topic in topics)
            {
                var consumer = CreateConsumer(topic);
                consumer.Start();
            }
        }

        private IConsumerBase CreateConsumer(string topic)
        {
            var consumer = Consumers.FirstOrDefault(c => c.Key == topic).Value;
            if (consumer != null) return consumer;

            consumer = new HttpPushOutgoingMessageConsumer(_kafkaConsumer,
                _nebulaService.NebulaContext,
                _loggerFactory, topic);
            Consumers.Add(new KeyValuePair<string, IConsumerBase>(topic, consumer));
            return consumer;
        }
    }
}
using System;
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
using Wormhole.Kafka;

namespace Wormhole.Worker
{
    public class ConsumerHostedService : IHostedService
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly NebulaService _nebulaService;
        private readonly IOptions<KafkaConfig> _options;
        private readonly ITenantDa _tenantDa;

        public ConsumerHostedService(IOptions<KafkaConfig> options, ITenantDa tenantDa, NebulaService nebulaService, ILoggerFactory loggerFactory)
        {
            _options = options;
            _tenantDa = tenantDa;
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
            if (topics == null || topics.Count < 1)
                throw new Exception("There is no topic for message consumption");

            foreach (var topic in topics)
            {
                var consumer = CreateConsumer(topic);
                consumer.Start();
            }
        }

        private IKafkaConsumer<Null, string> CreateConsumer(string topic)
        {
            return new HttpPushOutgoingMessageConsumer(_options,
                _nebulaService,
                _loggerFactory, topic);
        }
    }
}
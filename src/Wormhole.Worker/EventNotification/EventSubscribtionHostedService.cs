﻿using System;
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
using Wormhole.Interface;
using Wormhole.Kafka;

namespace Wormhole.Worker
{
    public class EventSubscribtionHostedService : IHostedService
    {

        private const string TopicName = "wormhole.events";
        private readonly ILoggerFactory _loggerFactory;
        private readonly IOptions<KafkaConfig> _options;
        private readonly NebulaService _nebulaService;

        public EventSubscribtionHostedService(IOptions<KafkaConfig> options, ILoggerFactory loggerFactory,
            NebulaService nebulaService)
        {
            _options = options;
            _loggerFactory = loggerFactory;
            _nebulaService = nebulaService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            StartConsuming();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private void StartConsuming()
        {
            var consumer = CreateConsumer(TopicName);
            consumer.Start();
        }

        private IKafkaConsumer<Null, string> CreateConsumer(string topic)
        {
            return new EventConsumer(_options,
                _loggerFactory, topic, _nebulaService);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nebula.Queue.Implementation;
using Newtonsoft.Json;
using Wormhole.Api.Model.PublishModel;
using Wormhole.Configurations;
using Wormhole.Job;
using Wormhole.Kafka;

namespace Wormhole.Worker
{
    public class HttpPushOutgoingMessageConsumer : KafkaConsumer
    {
        private readonly ILogger _logger;
        private readonly NebulaService _nebulaService;


        public HttpPushOutgoingMessageConsumer(IOptions<KafkaConfig> options, NebulaService nebulaService,
            ILoggerFactory logger, string topicName) : base(options, logger,
            ConsumerDiagnosticProvider.GetStat(typeof(HttpPushOutgoingMessageConsumer).FullName, topicName), topicName)
        {
            _nebulaService = nebulaService;
            _logger = logger.CreateLogger(nameof(HttpPushOutgoingMessageConsumer));
            Initialize(new List<KeyValuePair<string, object>>(), MessageReceived);
        }

        private void MessageReceived(object sender, Message<Null, string> message)
        {
            var publishInput = JsonConvert.DeserializeObject<PublishInput>(message.Value);
            if (!publishInput.ValidateTags())
            {
                _logger.LogWarning("Received message with invalid tags", publishInput);
                return;
            }

            if (string.IsNullOrWhiteSpace(publishInput.Category))
            {
                _logger.LogWarning("Received message with invalid category", publishInput);
                return;
            }

            var jobIdTagPairs =
                _nebulaService.GetJobIdTagPairs(publishInput.Tenant, publishInput.Category, publishInput.Tags);

            if (!jobIdTagPairs.Any())
            {
                _logger.LogWarning($"active job not found for " +
                    $"Tenant: {publishInput.Tenant} " +
                    $"Category: {publishInput.Category} " +
                    $"Tags: {{{string.Join(", ", publishInput.Tags)}}}");
                return;
            }

            var step = new HttpPushOutgoingQueueStep
            {
                Payload = publishInput.Payload.ToString(),
                Category = publishInput.Category,
                StepId = Guid.NewGuid().ToString()
            };

            foreach (var (key, value) in jobIdTagPairs)
            {
                step.Tag = value;
                var queue = _nebulaService.NebulaContext
                    .JobStepSourceBuilder
                    .BuildDelayedJobQueue<HttpPushOutgoingQueueStep>(key);

                _logger.LogInformation($"queueing message: {message.Value}");
                queue.Enqueue(step, DateTime.UtcNow).GetAwaiter().GetResult();
            }
        }
    }
}
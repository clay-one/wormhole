using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Nebula;
using Nebula.Queue.Implementation;
using Newtonsoft.Json;
using Wormhole.Api.Model;
using Wormhole.Job;
using Wormhole.Kafka;

namespace Wormhole.Worker
{
    public class HttpPushOutgoingMessageConsumer : ConsumerBase, IDisposable
    {
        private readonly NebulaContext _nebulaContext;


        public HttpPushOutgoingMessageConsumer(IKafkaConsumer<Null, string> consumer, NebulaContext nebulaContext,
            ILoggerFactory logger, string topicName) : base(consumer, logger,
            ConsumerDiagnosticProvider.GetStat(typeof(HttpPushOutgoingMessageConsumer).FullName, topicName))
        {
            _nebulaContext = nebulaContext;
            Topic = topicName;
            ICollection<KeyValuePair<string, object>> config = new Collection<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("group.id", "GroupId")
            };

            consumer.Initialize(config, MessageReceived);
        }


        public override string Topic { get; }

        private void MessageReceived(object sender, Message<Null, string> message)
        {
            Stopwatch sw;
            sw = Stopwatch.StartNew();

            Logger.LogTrace($"{DateTime.Now}_Start of pushing to nebula");

            var publishInput = JsonConvert.DeserializeObject<PublishInput>(message.Value);
            if (publishInput.Tags == null || publishInput.Tags.Count < 1)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(publishInput.Category))
            {
                return;
            }

            var jobIdTagPairs =
                NebulaWorker.GetJobIdTagPairs(publishInput.Tenant, publishInput.Category, publishInput.Tags);
            if (jobIdTagPairs == null)
            {
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
                var queue = _nebulaContext
                    .JobStepSourceBuilder
                    .BuildDelayedJobQueue<HttpPushOutgoingQueueStep>(key);

                Logger.LogInformation($"queueing message: {message.Value}");
                queue.Enqueue(step, DateTime.UtcNow).GetAwaiter().GetResult();
            }

            Logger.LogTrace($"{DateTime.Now}_End of pushing to nebula_Time taken: {sw.ElapsedMilliseconds}");
            sw.Stop();
        }
    }
}
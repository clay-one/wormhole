﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Nebula;
using Nebula.Queue;
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
        private readonly string _topicName;
        

        public HttpPushOutgoingMessageConsumer(IKafkaConsumer<Null, string> consumer,  NebulaContext nebulaContext, ILoggerFactory logger, string topicName) : base(consumer, logger, ConsumerDiagnosticProvider.GetStat(typeof(HttpPushOutgoingMessageConsumer).FullName, topicName))
        {
            _nebulaContext = nebulaContext;
            _topicName = topicName;
            ICollection<KeyValuePair<string, object>> config = new Collection<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("group.id","GroupId")
            };

            consumer.Initialize(config, OnMessageRecived);
        }
        

        public override string Topic => _topicName;

        private void OnMessageRecived(object sender, Message<Null, string> message)
        {
            var publishInput = JsonConvert.DeserializeObject<PublishInput>(message.Value);
            if (publishInput.Tags == null || publishInput.Tags.Count <1)
                return;

            if (string.IsNullOrWhiteSpace(publishInput.Category))
                return;

            var jobIdTagPairs = NebulaWorker.GetJobIdTagPairs(publishInput.Tenant, publishInput.Category, publishInput.Tags);
            if (jobIdTagPairs == null)
                return;

            var step = new HttpPushOutgoingQueueStep
            {
                Payload = publishInput.Payload.ToString(),
                Category = publishInput.Category,
                StepId = Guid.NewGuid().ToString()
            };

            foreach (var pair in jobIdTagPairs)
            {
                step.Tag = pair.Value;
                var queue =
                    _nebulaContext.JobStepSourceBuilder.BuildDelayedJobQueue<HttpPushOutgoingQueueStep>(pair.Key);
                Logger.LogInformation($"enquing message : {message.Value} ");
                queue.Enqueue(step, DateTime.UtcNow).GetAwaiter().GetResult();
            }
        }
    }
}
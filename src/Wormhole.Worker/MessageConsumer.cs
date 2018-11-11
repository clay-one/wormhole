using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AutoMapper.Mappers;
using ComposerCore.Attributes;
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
    public class MessageConsumer : ConsumerBase
    {
        private readonly NebulaContext _nebulaContext;
        private readonly string _jobId;
        private readonly string _topicName;
        

        public MessageConsumer(IKafkaConsumer<Null, string> consumer,  NebulaContext nebulaContext, ILoggerFactory logger, string topicName, string jobId) : base(consumer, logger, ConsumerDiagnosticProvider.GetStat(typeof(MessageConsumer).FullName, topicName))
        {
            _nebulaContext = nebulaContext;
            _jobId = jobId;
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
            Logger.LogDebug(message.Value);
            var publishInput = JsonConvert.DeserializeObject<PublishInput>(message.Value);
            var step = new OutgoingQueueStep
            {
                Payload = publishInput.Payload.ToString(),
                Category = publishInput.Category
            };

            var queue = _nebulaContext.GetDelayedJobQueue<OutgoingQueueStep>(QueueType.Delayed);
            queue.Enqueue(step, DateTime.UtcNow, _jobId).GetAwaiter().GetResult();
        }
    }
}
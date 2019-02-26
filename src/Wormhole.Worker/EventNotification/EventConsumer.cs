using System.Collections.Generic;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Wormhole.Configurations;
using Wormhole.DomainModel;
using Wormhole.Kafka;

namespace Wormhole.Worker.EventNotification
{
    public class EventConsumer : KafkaConsumer
    {
        private readonly ILogger _logger;
        private readonly NebulaService _nebulaService;

        public EventConsumer(IOptions<KafkaConfig> options,
            ILoggerFactory logger, string topicName, NebulaService nebulaService) : base(options, logger,
            ConsumerDiagnosticProvider.GetStat(typeof(EventConsumer).FullName, topicName), topicName)
        {
            _nebulaService = nebulaService;
            _logger = logger.CreateLogger(nameof(EventConsumer));
            Initialize(new List<KeyValuePair<string, object>>(), MessageReceived);
        }

        private void MessageReceived(object sender, Message<Null, string> message)
        {
            var modificationInfo = JsonConvert.DeserializeObject<OutputChannelModificationInfo>(message.Value, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
            var eventSubscriber = GetSubscriber(modificationInfo.ModificationType);
            eventSubscriber.Subscribe(modificationInfo, _nebulaService);
        }

        private IOutputChannelEventSubscriber GetSubscriber(OutputChannelModificationType modificationType)
        {
            if (modificationType == OutputChannelModificationType.ADD)
            {
                return new OutputChannelAddEventSubscriber();
            }

            return null;
        }
    }
}
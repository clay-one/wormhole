using System.Collections.Generic;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Wormhole.Api.Model.Publish;
using Wormhole.Configurations;
using Wormhole.DomainModel;
using Wormhole.Interface;
using Wormhole.Kafka;

namespace Wormhole.Worker
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
            var modificationInfo = JsonConvert.DeserializeObject<OutputChannelModificationInfo>(message.Value);
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
using System.Collections.Generic;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Wormhole.Api.Model.PublishModel;
using Wormhole.Configurations;
using Wormhole.Interface;
using Wormhole.Kafka;

namespace Wormhole.InputChannels.Kafka.Consumer
{
    public class KafkaInputChannelConsumer : KafkaConsumer
    {
        private readonly ILogger _logger;
        private readonly IPublishMessageLogic _publishMessageLogic;

        public KafkaInputChannelConsumer(IOptions<KafkaConfig> options,
            ILoggerFactory logger, string topicName, IPublishMessageLogic publishMessageLogic) : base(options, logger,
            ConsumerDiagnosticProvider.GetStat(typeof(KafkaInputChannelConsumer).FullName, topicName), topicName)
        {
            _publishMessageLogic = publishMessageLogic;
            _logger = logger.CreateLogger(nameof(KafkaInputChannelConsumer));
            Initialize(new List<KeyValuePair<string, object>>(), MessageReceived);
        }

        private void MessageReceived(object sender, Message<Null, string> message)
        {
            var publishInput = JsonConvert.DeserializeObject<PublishInput>(message.Value);

            if (publishInput.Tags == null || publishInput.Tags.Count < 1)
            {
                _logger.LogInformation(
                    $"KafkaInputChannelConsumer_MessageReceived: Tags don't exist. publishInput: {publishInput}");
                return;
            }

            if (string.IsNullOrWhiteSpace(publishInput.Category))
            {
                _logger.LogInformation(
                    $"KafkaInputChannelConsumer_MessageReceived: Category doesn't exist. publishInput: {publishInput}");
                return;
            }

            var result = _publishMessageLogic.ProduceMessage(publishInput);

            _logger.LogInformation(
                $"KafkaInputChannelConsumer_MessageReceived: publishMessage logic called: publishInput: {publishInput}, result: {result}");
        }
    }
}
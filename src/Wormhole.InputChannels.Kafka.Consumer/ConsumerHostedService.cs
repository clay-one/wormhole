using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wormhole.Configurations;
using Wormhole.Interface;
using Wormhole.Kafka;

namespace Wormhole.InputChannels.Kafka.Consumer
{
    public class ConsumerHostedService : IHostedService
    {
        private const string TopicName = "wormhole.channels.publish";
        private readonly ILoggerFactory _loggerFactory;
        private readonly IOptions<KafkaConfig> _options;
        private readonly IPublishMessageLogic _publishMessageLogic;

        public ConsumerHostedService(IOptions<KafkaConfig> options, ILoggerFactory loggerFactory,
            IPublishMessageLogic publishMessageLogic)
        {
            _options = options;
            _loggerFactory = loggerFactory;
            _publishMessageLogic = publishMessageLogic;
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
            return new KafkaInputChannelConsumer(_options,
                _loggerFactory, topic, _publishMessageLogic);
        }
    }
}
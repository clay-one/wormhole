using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Wormhole.Api.Model;
using Wormhole.DTO;
using Wormhole.Interface;
using Wormhole.Kafka;

namespace Wormhole.Logic
{
    public class PublishMessageLogic : IPublishMessageLogic
    {
        private static HttpClient _httpClient;
        private readonly IKafkaProducer _producer;

        private ILogger<PublishMessageLogic> Logger { get; set; }

        public PublishMessageLogic(IKafkaProducer producer, ILogger<PublishMessageLogic> logger)
        {
            _producer = producer;
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
            Logger = logger;
        }

        public async Task<ProduceMessageOutput> ProduceMessage(PublishInput input)
        {
            try
            {
                _producer.Produce(input);

                return new ProduceMessageOutput
                {
                    Success = true
                };
            }
            catch (Exception ex)
            {
                Logger.LogError($"PublishMessageLogic - ProduceMessage: {ex.Message}",
                    ex);

                return new ProduceMessageOutput
                {
                    Success = false,
                    Error = ErrorKeys.UnableProduceMessage
                };
            }
        }
    }
}
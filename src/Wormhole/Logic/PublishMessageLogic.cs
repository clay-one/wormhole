using System;
using Microsoft.Extensions.Logging;
using Wormhole.Api.Model.PublishModel;
using Wormhole.DTO;
using Wormhole.Interface;
using Wormhole.Kafka;

namespace Wormhole.Logic
{
    public class PublishMessageLogic : IPublishMessageLogic
    {
        private readonly IKafkaProducer _producer;

        public PublishMessageLogic(IKafkaProducer producer, ILogger<PublishMessageLogic> logger)
        {
            _producer = producer;
            Logger = logger;
        }

        private ILogger<PublishMessageLogic> Logger { get; }

        public ProduceMessageOutput ProduceMessage(PublishInput input)
        {
            try
            {
                _producer.Produce(input.Tenant, input);

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
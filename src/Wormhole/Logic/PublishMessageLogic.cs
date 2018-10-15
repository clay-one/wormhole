using System;
using Wormhole.Api.Model;
using Wormhole.Kafka;
using Wormhole.Models;

namespace Wormhole.Logic
{
    public class PublishMessageLogic : IPublishMessageLogic
    {
        private readonly IKafkaProducer _producer;

        public PublishMessageLogic(IKafkaProducer producer)
        {
            _producer = producer;
        }
        public ProduceMessageOutput ProduceMessage(PublishInput input)
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
                return new ProduceMessageOutput
                {
                    
                    Success = false,
                    Error = ErrorKeys.UnableProduceMessage
                };
            }              
        }
    }
}
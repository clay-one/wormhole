using System;
using Wormhole.Api.Model;
using Wormhole.Kafka;

namespace Wormhole.Logic
{
    public class PublishMessage : IPublishMessage
    {
        public void ProduceMessage(PublishInput input)
        {
            try
            {
                var producer = new KafkaProducer<PublishInput>();
                producer.Produce(input);
            }
            catch (Exception)
            {
                
            }
                
        }
    }
}
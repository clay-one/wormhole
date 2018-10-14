using Wormhole.Api.Model;

namespace Wormhole.Kafka
{
    public interface IKafkaProducer
    {
        void Produce(PublishInput message);

        int Flush();
    }
}
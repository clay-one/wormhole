using System.Threading.Tasks;
using Wormhole.Api.Model;

namespace Wormhole.Kafka
{
    public interface IKafkaProducer
    {
        void Produce(string topic, IKafkaMessage message);
        Task ProduceAsync(string topic, IKafkaMessage message);

        int Flush();
    }
}
using System.Threading.Tasks;
using Wormhole.Api.Model;

namespace Wormhole.Kafka
{
    public interface IKafkaProducer
    {
        void Produce(PublishInput message);
        Task ProduceAsync(PublishInput message);

        int Flush();
    }
}
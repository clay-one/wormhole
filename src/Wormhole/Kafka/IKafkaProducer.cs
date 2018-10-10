namespace Wormhole.Kafka
{
    public interface IKafkaProducer<in T> where T : class
    {
        void Produce(T message);

        int Flush();
    }
}
using System.Collections.Generic;
using System.Text;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Newtonsoft.Json;

namespace Wormhole.Kafka
{
    public class KafkaProducer<T> : IKafkaProducer<T> where T : class
    {
        private readonly IDeliveryHandler<Null, string> _deliverHandler;
        private readonly Producer _producer;
        private readonly ISerializingProducer<Null, string> _serializingProducer;


        public KafkaProducer()
        {
            var config = new Dictionary<string, object>
            {
                {"bootstrap.servers", "172.30.3.59:9091"},
                {"delivery.report.only.error", true}
            };

            _producer = new Producer(config);
            _serializingProducer =
                _producer.GetSerializingProducer(new NullSerializer(), new StringSerializer(Encoding.UTF8));
        }

        public void Produce(T message)
        {
            var serializedObject = JsonConvert.SerializeObject(message, Formatting.None);
            _serializingProducer.ProduceAsync("wormhole.test", null, serializedObject);
        }

        public int Flush()
        {
            return _producer.Flush(10000);
        }
    }
}
using System.Collections.Generic;
using System.Text;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Wormhole.Api.Model;

namespace Wormhole.Kafka
{
    public class KafkaProducer : IKafkaProducer
    {
        private readonly IDeliveryHandler<Null,string> _deliverHandler;
        private readonly Producer _producer;
        private readonly ISerializingProducer<Null, string> _serializingProducer;
        private readonly KafkaConfig _config;
        

        public KafkaProducer(IOptions<KafkaConfig> options)
        {
            _config = options.Value;

            var config = new Dictionary<string, object>
            {
                {"bootstrap.servers", _config.ServerAddress},
                {"delivery.report.only.error", true}
            };
         
            _producer = new Producer(config);
            _serializingProducer =
                _producer.GetSerializingProducer(new NullSerializer(), new StringSerializer(Encoding.UTF8));
        }

        public void Produce(PublishInput message)
        {
            var serializedObject = JsonConvert.SerializeObject(message, Formatting.None);
            _serializingProducer.ProduceAsync(message.Tenant, null, serializedObject);
        }

        public int Flush()
        {
            return _producer.Flush(10000);
        }
    }
}
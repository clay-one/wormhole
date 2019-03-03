using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Wormhole.Api.Model;
using Wormhole.Configurations;

namespace Wormhole.Kafka
{
    public class KafkaProducer : IKafkaProducer
    {
        private readonly Producer<Null, string> _producer;
        private readonly KafkaConfig _config;
        

        public KafkaProducer(IOptions<KafkaConfig> options)
        {
            _config = options.Value;

            var config = new Dictionary<string, object>
            {
                {"bootstrap.servers", _config.BootstrapServers},
                {"delivery.report.only.error", false}
            };
         
            _producer = new Producer<Null, string>(config, null, new StringSerializer(Encoding.UTF8));
        }

        public void Produce(string topic, IKafkaMessage message)
        {
#pragma warning disable 4014
            ProduceAsync(topic, message);
#pragma warning restore 4014
        }

        public async Task ProduceAsync(string topic, IKafkaMessage message)
        {
            var serializedObject = JsonConvert.SerializeObject(message, Formatting.None);
            await _producer.ProduceAsync(topic, null, serializedObject);
        }

        public int Flush()
        {
            return _producer.Flush(10000);
        }
    }
}
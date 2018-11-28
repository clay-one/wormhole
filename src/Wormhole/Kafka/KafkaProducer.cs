using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Wormhole.Api.Model;

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
                {"bootstrap.servers", _config.ServerAddress},
                {"delivery.report.only.error", false}
            };
         
            _producer = new Producer<Null, string>(config, null, new StringSerializer(Encoding.UTF8));
        }

        public void Produce(PublishInput message)
        {
#pragma warning disable 4014
            ProduceAsync(message);
#pragma warning restore 4014
        }

        public async Task ProduceAsync(PublishInput message)
        {
            var serializedObject = JsonConvert.SerializeObject(message, Formatting.None);
            await _producer.ProduceAsync(message.Tenant, null, serializedObject);
        }

        public int Flush()
        {
            return _producer.Flush(10000);
        }
    }
}
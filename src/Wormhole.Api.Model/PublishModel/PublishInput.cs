using System.Collections.Generic;

namespace Wormhole.Api.Model.PublishModel
{
    public class PublishInput : IKafkaMessage
    {
        public object Payload { get; set; }
        public string Tenant { get; set; }
        public string Category { get; set; }
        public IList<string> Tags { get; set; }
    }
}

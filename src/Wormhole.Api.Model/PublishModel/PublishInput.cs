using System.Collections.Generic;

namespace Wormhole.Api.Model.Publish
{
    public class PublishInput
    {
        public object Payload { get; set; }
        public string Tenant { get; set; }
        public string Category { get; set; }
        public IList<string> Tags { get; set; }
    }
}

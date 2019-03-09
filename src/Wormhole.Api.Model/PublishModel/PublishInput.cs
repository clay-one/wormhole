using System.Collections.Generic;
using System.Linq;

namespace Wormhole.Api.Model.PublishModel
{
    public class PublishInput : IKafkaMessage
    {
        public object Payload { get; set; }
        public string Tenant { get; set; }
        public string Category { get; set; }
        public IList<string> Tags { get; set; }
    }

    public static class PublishInputValidators
    {
        public static bool ValidateTags(this PublishInput self)
        {
            return self?
                .Tags?
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Any() ?? false;
        }
    }
}

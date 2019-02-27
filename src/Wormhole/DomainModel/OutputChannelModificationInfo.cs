using Wormhole.Api.Model;

namespace Wormhole.DomainModel
{
    public class OutputChannelModificationInfo : IKafkaMessage
    {
        public OutputChannel.OutputChannel OutputChannel { get; set; }
        public OutputChannelModificationType ModificationType { get; set; }
    }
}

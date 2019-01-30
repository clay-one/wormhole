namespace Wormhole.Api.Model.OutputChannels
{
    public class KafkaOutputChannelAddResponse : OutputChannelAddResponse
    {
        public string TopicId { get; set; }
    }
}
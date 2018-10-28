namespace Wormhole.DomainModel
{
    public class KafkaOutputChannelSpecification : OutputChannelSpecification
    {
        public string TargetTopic { get; set; } 
    }
}
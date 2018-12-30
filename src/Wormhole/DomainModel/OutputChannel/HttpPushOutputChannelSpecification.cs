namespace Wormhole.DomainModel
{
    public class HttpPushOutputChannelSpecification : OutputChannelSpecification
    {
        public string TargetUrl { get; set; }

        public bool PayloadOnly { get; set; }
    }
}
namespace Wormhole.Api.Model
{
    public class PublishInput
    {
        public object Payload { get; set; }
        public string Tenant { get; set; }
        public string Category { get; set; }
    }
}

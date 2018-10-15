namespace Wormhole.Api.Model
{
    public class PublishInput
    {
        public object Message { get; set; }
        public string Tenant { get; set; }
        public string Category { get; set; }
    }
}

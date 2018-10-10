namespace Wormhole.Api.Model
{
    public class PublishInput
    {
        public string Message { get; set; }
        public Tenants Tenant { get; set; }
        public Categories Category { get; set; }
    }
}

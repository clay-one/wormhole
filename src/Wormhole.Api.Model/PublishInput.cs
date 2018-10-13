namespace Wormhole.Api.Model
{
    public class PublishInput
    {
        public object Message { get; set; }
        public Tenants Tenant { get; set; }
        public Categories Category { get; set; }
    }
}

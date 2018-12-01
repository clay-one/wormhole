using System.Net;

namespace Wormhole.DomainModel
{
    public class PublishMessageOutput
    {
        public string ErrorMessage { get; set; }
        public HttpStatusCode HttpResultCode { get; set; }
    }
}
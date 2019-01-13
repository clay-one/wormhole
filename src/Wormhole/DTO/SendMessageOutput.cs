using System;
using System.Net;

namespace Wormhole.DTO
{
    public class SendMessageOutput : BaseOutput
    {
        public HttpStatusCode HttpResponseCode { get; set; }
        public DateTime ResponseTime { get; set; }
    }
}
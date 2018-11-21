using Wormhole.Api.Model;
using Wormhole.DomainModel;
using Wormhole.Job;

namespace Wormhole.Mapping.Profile
{
    public class HttpPushOutgoingQueueParametersProfile  : global::AutoMapper.Profile
    {
        public HttpPushOutgoingQueueParametersProfile()
        {
            CreateMap<HttpPushOutputChannelSpecification, HttpPushOutgoingQueueParameters>();
        }
    }
}
using Wormhole.Api.Model;
using Wormhole.DomainModel;

namespace Wormhole.Mapping.Profile
{
    public class InputChannelAddRequestProfile : global::AutoMapper.Profile
    {
        public InputChannelAddRequestProfile()
        {
            CreateMap<HttpPushInputputChannelAddRequest, InputChannel>();
        }
    }
}
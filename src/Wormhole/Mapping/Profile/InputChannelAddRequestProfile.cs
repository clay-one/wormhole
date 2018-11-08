using Wormhole.Api.Model;
using Wormhole.DomainModel.InputChannel;

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
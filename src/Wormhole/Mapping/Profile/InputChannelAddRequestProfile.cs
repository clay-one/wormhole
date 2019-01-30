using Wormhole.Api.Model.InputChannels;
using Wormhole.DomainModel;

namespace Wormhole.Mapping.Profile
{
    public class InputChannelAddRequestProfile : global::AutoMapper.Profile
    {
        public InputChannelAddRequestProfile()
        {
            CreateMap<InputChannelAddRequest, InputChannel>();
        }
    }
}
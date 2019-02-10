using Wormhole.Api.Model.InputChannels;
using Wormhole.DomainModel;

namespace Wormhole.Mapping.Profile
{
    public class InputChannelAddResponseProfile : global::AutoMapper.Profile
    {
        public InputChannelAddResponseProfile()
        {
            CreateMap<InputChannel,InputChannelAddResponse>();
        }
    }
}

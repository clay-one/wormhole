using Wormhole.Api.Model;
using Wormhole.DomainModel.InputChannel;

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

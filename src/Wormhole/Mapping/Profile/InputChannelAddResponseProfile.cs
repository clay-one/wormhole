using AutoMapper;
using Wormhole.Api.Model;
using Wormhole.DomainModel;

namespace Wormhole.Mapping.Profile
{
    public class InputChannelAddResponseProfile : global::AutoMapper.Profile
    {
        public InputChannelAddResponseProfile()
        {
            CreateMap<InputChannel,InputChannelAddResponseProfile>();
        }
    }
}
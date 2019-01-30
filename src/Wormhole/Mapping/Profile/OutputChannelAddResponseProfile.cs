using AutoMapper;
using Wormhole.Api.Model.OutputChannels;
using Wormhole.DomainModel;

namespace Wormhole.Mapping.Profile
{
    public class OutputChannelAddResponseProfile<T> : global::AutoMapper.Profile where T : OutputChannelAddResponse
    {
        protected readonly IMappingExpression<OutputChannel, T> Expression;
        public OutputChannelAddResponseProfile()
        {
            Expression = CreateMap<OutputChannel, T>().ForMember(d => d.Category,
                opt => opt.MapFrom(src => src.FilterCriteria.Category)).ForMember(d => d.Tag,
                opt => opt.MapFrom(src => src.FilterCriteria.Tag));
        }
    }
}

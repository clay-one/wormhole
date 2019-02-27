using AutoMapper;
using Wormhole.Api.Model.OutputChannels;
using Wormhole.DomainModel;
using Wormhole.DomainModel.OutputChannel;

namespace Wormhole.Mapping.Profile
{
    public class OutputChannelAddRequestProfile<TSource> : global::AutoMapper.Profile where TSource : OutputChannelAddRequest
    {
        protected readonly IMappingExpression<TSource, OutputChannel> Expression;

        public OutputChannelAddRequestProfile()
        {
            Expression = CreateMap<TSource, OutputChannel>().ForMember(d => d.FilterCriteria,
                opt => opt.MapFrom(src => new MessageFilterCriteria { Category = src.Category, Tag = src.Tag }));
        }

    }
}
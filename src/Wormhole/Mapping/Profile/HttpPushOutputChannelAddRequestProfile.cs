using AutoMapper;
using Wormhole.Api.Model;
using Wormhole.DomainModel;

namespace Wormhole.Mapping.Profile
{
    public class HttpPushOutputChannelAddRequestProfile : OutputChannelAddRequestProfile<HttpPushOutputChannelAddRequest>
    {
        public HttpPushOutputChannelAddRequestProfile()
        {
            Expression.ForMember(d => d.ChannelType, opt => opt.UseValue(ChannelType.HttpPush)).ForMember(
                    d => d.TypeSpecification,
                    opt => opt.MapFrom(src =>
                    
                         new HttpPushOutputChannelSpecification()
                        {
                            PayloadOnly = src.PayloadOnly,
                            TargetUrl = src.TargetUrl
                        }
                    ));
        }
    }
}
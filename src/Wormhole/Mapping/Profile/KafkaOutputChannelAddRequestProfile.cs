using AutoMapper;
using Wormhole.Api.Model;
using Wormhole.DomainModel;

namespace Wormhole.Mapping.Profile
{
    public class KafkaOutputChannelAddRequestProfile : OutputChannelAddRequestProfile<KafkaOutputChannelAddRequest>
    {
        public KafkaOutputChannelAddRequestProfile()
        {
            Expression.ForMember(d => d.ChannelType, opt => opt.UseValue(ChannelType.Kafka)).ForMember(
                    d => d.TypeSpecification,
                    opt => opt.MapFrom(src =>
                    
                         new KafkaOutputChannelSpecification
                         {
                            TargetTopic = src.TopicId
                        }
                    ));
        }
    }
}
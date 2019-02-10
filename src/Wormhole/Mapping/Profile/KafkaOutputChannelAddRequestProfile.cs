using Wormhole.Api.Model.OutputChannels;
using Wormhole.DomainModel;

namespace Wormhole.Mapping.Profile
{
    public class KafkaOutputChannelAddRequestProfile : OutputChannelAddRequestProfile<KafkaOutputChannelAddRequest>
    {
        public KafkaOutputChannelAddRequestProfile()
        {
            Expression.ForMember(d => d.ChannelType, opt => opt.UseValue(DomainModel.ChannelType.Kafka)).ForMember(
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
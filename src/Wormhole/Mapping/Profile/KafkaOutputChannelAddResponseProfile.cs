using Wormhole.Api.Model;
using Wormhole.DomainModel;

namespace Wormhole.Mapping.Profile
{
    public class KafkaOutputChannelAddResponseProfile : OutputChannelAddResponseProfile<KafkaOutputChannelAddResponse>
    {
        public KafkaOutputChannelAddResponseProfile()
        {
            Expression.ForMember(
                d => d.TopicId,
                opt => opt.MapFrom(src =>
                    ((KafkaOutputChannelSpecification)src.TypeSpecification).TargetTopic
                ));
        }
    }
}

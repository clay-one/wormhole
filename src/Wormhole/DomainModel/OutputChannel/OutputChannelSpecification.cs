using MongoDB.Bson.Serialization.Attributes;

namespace Wormhole.DomainModel
{
    [BsonDiscriminator(Required = true)]
    [BsonKnownTypes(typeof(HttpPullOutputChannelSpecification), typeof(HttpPushOutputChannelSpecification), typeof(KafkaOutputChannelSpecification))]
    public abstract class OutputChannelSpecification
    {
    }
}
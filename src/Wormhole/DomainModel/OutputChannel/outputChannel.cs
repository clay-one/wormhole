using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Wormhole.DomainModel
{
    public class OutputChannel
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonRepresentation(BsonType.String)]
        public ChannelType ChannelType { get; set; }

        public string TenantId { get; set; }
        public bool HasMetaData { get; set; }
        public OutputChannelSpecification TypeSpecification { get; set; }
    }
}
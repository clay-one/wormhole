using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Wormhole.DomainModel
{
    public class InputChannel
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string ExternalKey { get; set; }
        public string TenantId { get; set; }

        [BsonRepresentation(BsonType.String)]
        public ChannelType ChannelType { get; set; }
        public InputChannelSpecification TypeSpecification { get; set; }
    }
}
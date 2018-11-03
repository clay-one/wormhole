using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Wormhole.DomainModel
{
    public class OutputChannel
    {

        public OutputChannel()
        {
            Id = ObjectId.GenerateNewId();
            FilterCriteria = new MessageFilterCriteria();
        }

        [BsonId]
        public ObjectId Id { get; set; }

        public string ExternalKey { get; set; }

        [BsonRepresentation(BsonType.String)]
        public ChannelType ChannelType { get; set; }

        public string TenantId { get; set; }

        public OutputChannelSpecification TypeSpecification { get; set; }

        public MessageFilterCriteria FilterCriteria { get; set; }
    }
}
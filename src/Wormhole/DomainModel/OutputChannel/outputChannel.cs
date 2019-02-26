using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Wormhole.DomainModel.OutputChannel
{
    public class OutputChannel
    {

        public OutputChannel()
        {
            Id = ObjectId.GenerateNewId().ToString();
            FilterCriteria = new MessageFilterCriteria();
        }

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string ExternalKey { get; set; }

        [BsonRepresentation(BsonType.String)]
        public ChannelType ChannelType { get; set; }

        public string TenantId { get; set; }
        public string JobId { get; set; }

        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        public OutputChannelSpecification TypeSpecification { get; set; }

        public MessageFilterCriteria FilterCriteria { get; set; }
        public bool Equals(OutputChannel other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(ExternalKey, other.ExternalKey)
                   && Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((OutputChannel)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Id != null ? Id.GetHashCode() : 0) * 397) ^ (ExternalKey != null ? ExternalKey.GetHashCode() : 0);
            }
        }

        public static bool operator ==(OutputChannel left, OutputChannel right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(OutputChannel left, OutputChannel right)
        {
            return !Equals(left, right);
        }
    }
}
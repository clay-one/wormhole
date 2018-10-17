using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Wormhole.DomainModel
{
    public class Tenant
    {

        [BsonId]
        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public DateTime CreationTime { get; set; }

        [BsonRepresentation(BsonType.String)]
        public ChannelType InputChannel { get; set; }

        [BsonRepresentation(BsonType.String)]
        public ChannelType OutputChannel { get; set; }
    }
}
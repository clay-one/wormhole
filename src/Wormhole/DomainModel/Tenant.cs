using System;
using System.Collections.Generic;
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
        public List<ChannelType> InputChannels { get; set; }

        [BsonRepresentation(BsonType.String)]
        public List<ChannelType> OutputChannels { get; set; }
    }
}
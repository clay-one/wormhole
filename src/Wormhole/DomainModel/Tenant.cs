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
        public string Identifier { get; set; }
        public DateTime CreationTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}
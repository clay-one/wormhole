using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Wormhole.DomainModel
{
    public class OutgoingMessageLog
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string JobId { get; set; }
        public string JobStepIdentifier { get; set; }
        public object Payload { get; set; }
        public string Category { get; set; }
        public string Tag { get; set; }
        public int FailCount { get; set; }
        public DateTime ResponseTime { get; set; }
        public PublishMessageOutput PublishOutput { get; set; } 
    }
}
﻿using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Wormhole.DomainModel
{
    public class MessageLog
    {


        [BsonId]
        public ObjectId Id { get; set; }
        public object Payload { get; set; }
        public string Category { get; set; }
        public string Tag { get; set; }
        public int FailCount { get; set; }
        public string ErrorMessage { get; set; }
        public string HttpResultCode { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
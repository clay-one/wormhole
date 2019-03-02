namespace Wormhole.Utils
{
    public static class Constants
    {
        public const string MongoConnectionStringSection = "Mongo";
        public const string NebulaMongoConnectionString = "nebula:mongoConnectionString";
        public const string NebulaRedisConnectionString = "nebula:redisConnectionString";
        public const string NebulaConfigSection = "Nebula";
        public const string KafkaConfigSection = "Kafka";
        public const string RetryConfigSection = "RetryConfiguration";
        public const string ConnectionStringsConfigSection = "ConnectionStrings";
        public const string IdentityConfigSection = "Identity";
        public const string EventSourcingTopicName = "wormhole.events";
    }
}
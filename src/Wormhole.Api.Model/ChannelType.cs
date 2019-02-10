namespace Wormhole.Api.Model
{
    public enum ChannelType
    {
        Unknown = 0,
        HttpPush = 1,
        Kafka = 2,
        HttpPull = 3,
        RedisQueue = 4
    }
}
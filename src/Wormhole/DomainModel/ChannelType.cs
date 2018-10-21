namespace Wormhole.DomainModel
{
    public enum ChannelType
    {
        HttpPush =0,
        HttpPull =1,
        KafkaTopic=2,
        RedisQueue=3
    }
}
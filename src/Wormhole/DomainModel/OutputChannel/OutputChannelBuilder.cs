using System;

namespace Wormhole.DomainModel
{
    public static class OutputChannelBuilder
    {
        public static OutputChannel CreateKafkaOutputChannel(string targetTopicId,
            bool hasMetaData, string tenantId)
        {
            if (string.IsNullOrWhiteSpace(targetTopicId) || string.IsNullOrWhiteSpace(tenantId))
                throw new ArgumentNullException();

            return new OutputChannel
            {
                ChannelType = ChannelType.Kafka,
                TypeSpecification = new KafkaOutputChannelSpecification
                {
                    TargetTopic = targetTopicId
                },
                HasMetaData = hasMetaData,
                TenantId = tenantId
            };
        }

        public static OutputChannel CreateHttpPushOutputChannel(string targetUrl,
            bool hasMetaData, string tenantId)
        {
            if (string.IsNullOrWhiteSpace(targetUrl) || string.IsNullOrWhiteSpace(tenantId))
                throw new ArgumentNullException();

            return new OutputChannel
            {
                ChannelType = ChannelType.HttpPush,
                TypeSpecification = new HttpPushOutputChannelSpecification
                {
                    TargetUrl = targetUrl
                },
                HasMetaData = hasMetaData,
                TenantId = tenantId
            };
        }

        public static OutputChannel CreateHttpPullOutputChannel(bool hasMetaData, string tenantId)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
                throw new ArgumentNullException();

            return new OutputChannel
            {
                ChannelType = ChannelType.HttpPull,
                TypeSpecification = new HttpPullOutputChannelSpecification(),
                HasMetaData = hasMetaData,
                TenantId = tenantId
            };
        }
    }
}
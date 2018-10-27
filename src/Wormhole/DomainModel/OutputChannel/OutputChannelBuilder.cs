using System;

namespace Wormhole.DomainModel
{
    public static class OutputChannelBuilder
    {
        public static OutputChannel CreateKafkaOutputChannel(string externalKey, string tenantId, string category, string tag, string targetTopicId)
        {
            if (string.IsNullOrWhiteSpace(targetTopicId) || string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(externalKey))
                throw new ArgumentNullException();

            var channel = CreateOutputChannel(externalKey, tenantId, category, tag);
            channel.ChannelType = ChannelType.Kafka;
            channel.TypeSpecification = new KafkaOutputChannelSpecification
            {
                TargetTopic = targetTopicId
            };
            return channel;
        }

        public static OutputChannel CreateHttpPushOutputChannel(string externalKey, string tenantId, string category, string tag , string targetUrl, bool payloadOnly)
        {
            if (string.IsNullOrWhiteSpace(targetUrl) || string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(externalKey))
                throw new ArgumentNullException();

            var channel = CreateOutputChannel(externalKey, tenantId, category, tag);
            channel.ChannelType = ChannelType.HttpPush;
            channel.TypeSpecification = new HttpPushOutputChannelSpecification
            {
                TargetUrl = targetUrl,
                PayloadOnly = payloadOnly
            };
            return channel;
        }

        public static OutputChannel CreateHttpPullOutputChannel(string externalKey, string tenantId,string category, string tag = null)
        {
            if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(externalKey))
                throw new ArgumentNullException();

            var channel = CreateOutputChannel(externalKey, tenantId, category, tag);
            channel.ChannelType = ChannelType.HttpPull;
            channel.TypeSpecification = new HttpPullOutputChannelSpecification();
            return channel;
        }

        private static OutputChannel CreateOutputChannel(string externalKey, string tenantId, string category, string tag)
        {
            return new OutputChannel
            {
                ExternalKey = externalKey,
                FilterCriteria = new MessageFilterCriteria {Category = category,Tag = tag},
                TenantId = tenantId
            };
        }
    }
}
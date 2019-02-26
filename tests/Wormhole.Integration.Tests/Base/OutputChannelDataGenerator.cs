using MongoDB.Driver;
using Nebula.Storage.Model;
using Wormhole.DataImplementation;
using Wormhole.DomainModel;
using Wormhole.DomainModel.OutputChannel;

namespace Wormhole.Integration.Tests.Base
{
    internal class OutputChannelDataGenerator
    {
        private readonly IMongoUtil _mongoUtil;

        public OutputChannelDataGenerator(IMongoUtil mongoUtil)
        {
            _mongoUtil = mongoUtil;
        }

        public OutputChannel AddHttpPushOutputChannel(string externalKey, string targetUrl, string category, string tag,
            string tenantId)
        {
            var outputChannel = new OutputChannel
            {
                ChannelType = ChannelType.HttpPush,
                TypeSpecification =
                    new HttpPushOutputChannelSpecification
                    {
                        PayloadOnly = true,
                        TargetUrl = targetUrl
                    },
                ExternalKey = externalKey,
                FilterCriteria = new MessageFilterCriteria {Category = category, Tag = tag},
                TenantId = tenantId
            };
            _mongoUtil.GetCollection<OutputChannel>(nameof(OutputChannel)).InsertOne(outputChannel);
            return outputChannel;
        }

        public void RemoveGenerated(string testOutputChannelKey)
        {
            var outputChannelFilter =
                Builders<OutputChannel>.Filter.Eq(nameof(OutputChannel.ExternalKey), testOutputChannelKey);
            var jobFilter = Builders<JobData>.Filter.Where(j => j.JobId.EndsWith(testOutputChannelKey));
            _mongoUtil.GetCollection<OutputChannel>(nameof(OutputChannel)).DeleteOne(outputChannelFilter);
            _mongoUtil.GetCollection<JobData>(nameof(JobData)).DeleteOne(jobFilter);
        }
    }
}
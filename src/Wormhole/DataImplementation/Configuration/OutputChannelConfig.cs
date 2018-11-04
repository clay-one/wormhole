using MongoDB.Driver;
using Wormhole.DomainModel;

namespace Wormhole.DataImplementation.Configuration
{
    public class OutputChannelConfig : IMongoCollectionConfig
    {
        public void Configure()
        {
            MongoUtil.CreateIndex("OutputChannel_ExternalKeyAndTenant_UniqueIndex",
                Builders<OutputChannel>.IndexKeys.Combine(
                    Builders<OutputChannel>.IndexKeys.Ascending(nameof(OutputChannel.ExternalKey)),
                    Builders<OutputChannel>.IndexKeys.Ascending(nameof(OutputChannel.TenantId))),
                new CreateIndexOptions<OutputChannel>
                {
                    Unique = true,
                });
        }
    }
}
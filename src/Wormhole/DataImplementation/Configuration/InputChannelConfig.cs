using MongoDB.Driver;
using Wormhole.DomainModel;

namespace Wormhole.DataImplementation.Configuration
{
    public class InputChannelConfig : IMongoCollectionConfig
    {
        public void Configure()
        {
            MongoUtil.CreateIndex("InputChannel_ExternalKeyAndTenant_UniqueIndex",
                Builders<InputChannel>.IndexKeys.Combine(
                    Builders<InputChannel>.IndexKeys.Ascending(nameof(InputChannel.ExternalKey)),
                    Builders<InputChannel>.IndexKeys.Ascending(nameof(InputChannel.TenantId))),
                new CreateIndexOptions<InputChannel>
                {
                    Unique = true,
                });
        }
    }
}
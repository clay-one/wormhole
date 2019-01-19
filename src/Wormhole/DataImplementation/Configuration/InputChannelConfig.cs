using MongoDB.Driver;
using Wormhole.DomainModel;

namespace Wormhole.DataImplementation.Configuration
{
    public class InputChannelConfig : IMongoCollectionConfig
    {
        private readonly IMongoUtil _mongoUtil;

        public InputChannelConfig(IMongoUtil mongoUtil)
        {
            _mongoUtil = mongoUtil;
        }

        public void Configure()
        {
            _mongoUtil.CreateIndex("InputChannel_ExternalKeyAndTenant_UniqueIndex",
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
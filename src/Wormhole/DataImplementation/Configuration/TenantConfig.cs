using MongoDB.Driver;
using Wormhole.DomainModel;

namespace Wormhole.DataImplementation.Configuration
{
    public class TenantConfig : IMongoCollectionConfig
    {
        private readonly IMongoUtil _mongoUtil;

        public TenantConfig(IMongoUtil mongoUtil)
        {
            _mongoUtil = mongoUtil;
        }
        public void Configure()
        {
            _mongoUtil.CreateIndex("TenantIdentifier_UniqueIndex",
                    Builders<Tenant>.IndexKeys.Ascending(nameof(Tenant.Identifier)),
                new CreateIndexOptions<Tenant>
                {
                    Unique = true,
                });
        }
    }
}




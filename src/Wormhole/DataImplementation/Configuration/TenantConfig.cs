using MongoDB.Driver;
using Wormhole.DomainModel;

namespace Wormhole.DataImplementation.Configuration
{
    public class TenantConfig : IMongoCollectionConfig
    {
        public void Configure()
        {
            MongoUtil.CreateIndex("TenantIdentifier_UniqueIndex",
                    Builders<Tenant>.IndexKeys.Ascending(nameof(Tenant.Identifier)),
                new CreateIndexOptions<Tenant>
                {
                    Unique = true,
                });
        }
    }
}




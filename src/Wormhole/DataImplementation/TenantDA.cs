using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using Wormhole.DomainModel;

namespace Wormhole.DataImplementation
{
    public class TenantDa : ITenantDa
    {
        private IMongoCollection<Tenant> TenantCollection
            => MongoUtil.GetCollection<Tenant>(nameof(Tenant));
        public async Task<List<Tenant>> FindTenants()
        {
            return await TenantCollection.Find(Builders<Tenant>.Filter.Empty).ToListAsync();
        }

        public async Task AddTenant(Tenant tenant)
        {
            await TenantCollection.InsertOneAsync(tenant);
        }
    }
}
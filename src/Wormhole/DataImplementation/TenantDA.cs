using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using Wormhole.DomainModel;

namespace Wormhole.DataImplementation
{
    public class TenantDA : ITenantDA
    {
        private IMongoCollection<Tenant> TenantCollection
            => MongoUtil.GetCollection<Tenant>(nameof(Tenant));
        public async Task<IList<Tenant>> FindTenants()
        {
            return await TenantCollection.Find(Builders<Tenant>.Filter.Empty).ToListAsync();
        }
    }
}
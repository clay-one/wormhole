using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using Wormhole.DomainModel;
using Wormhole.DTO;

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

        public async Task<AddTenantOutput> AddTenant(Tenant tenant)
        {                          
            try
            {
                await TenantCollection.InsertOneAsync(tenant);
                return new AddTenantOutput
                {
                    Success = true
                };
            }

            catch (MongoWriteException ex)
            {
                return new AddTenantOutput
                {
                    Success = false,
                    Error = ErrorKeys. DuplicateTenantIdentifier
                };
            }

            catch (Exception ex)
            {
                return new AddTenantOutput
                {
                    Success = false,
                    Error = ErrorKeys.InternalError
                };
            }
        }
    }
}
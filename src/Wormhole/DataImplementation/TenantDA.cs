using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Wormhole.DomainModel;
using Wormhole.DTO;

namespace Wormhole.DataImplementation
{
    public class TenantDa : ITenantDa
    {
        private readonly IMongoUtil _mongoUtil;

        private IMongoCollection<Tenant> TenantCollection
            => _mongoUtil.GetCollection<Tenant>(nameof(Tenant));

        private ILogger<TenantDa> Logger { get; set; }

        public TenantDa(IMongoUtil mongoUtil)
        {
            _mongoUtil = mongoUtil;
        }
        public TenantDa(ILogger<TenantDa> logger)
        {
            Logger = logger;
        }

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
                Logger.LogInformation($"TenantDa - AddTenant: Duplicate Tenant {ex.Message}", ex);
                return new AddTenantOutput
                {
                    Success = false,
                    Error = ErrorKeys. DuplicateTenantIdentifier
                };
            }

            catch (Exception ex)
            {
                Logger.LogError($"TenantDa - AddTenant: {ex.Message}", ex);
                return new AddTenantOutput
                {
                    Success = false,
                    Error = ErrorKeys.InternalError
                };
            }
        }
    }
}
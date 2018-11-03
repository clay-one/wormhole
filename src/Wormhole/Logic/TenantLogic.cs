using System;
using System.Threading.Tasks;
using Wormhole.DataImplementation;
using Wormhole.DomainModel;
using Wormhole.DTO;
using Wormhole.Interface;

namespace Wormhole.Logic
{
    public class TenantLogic : ITenantLogic
    {
        private readonly ITenantDa _tenantDa;

        public TenantLogic(ITenantDa tenantDa)
        {
            _tenantDa = tenantDa;
        }

        public async Task<AddTenantOutput> AddTenant(Tenant tenant)
        {
            if (string.IsNullOrWhiteSpace(tenant.Identifier) || string.IsNullOrWhiteSpace(tenant.Name))
            {
                return new AddTenantOutput
                {
                    Success = false,
                    Error = ErrorKeys.ParameterNull
                };
            }

            tenant.CreationTime = DateTime.UtcNow;

            var result = await _tenantDa.AddTenant(tenant);

            return result;
        }

        public Task<EditTenantOutput> EditTenant(Tenant tenant)
        {
            throw new System.NotImplementedException();
        }

        public Task<DeleteTenantOutput> DeleteTenant()
        {
            throw new System.NotImplementedException();
        }
    }
}

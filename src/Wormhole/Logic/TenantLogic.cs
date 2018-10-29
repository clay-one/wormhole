using System.Threading.Tasks;
using Wormhole.DomainModel;
using Wormhole.DTO;
using Wormhole.Interface;

namespace Wormhole.Logic
{
    public class TenantLogic : ITenantLogic
    {
        public Task<AddTenantOutput> AddTenant(Tenant tenant)
        {
            throw new System.NotImplementedException();
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

using System.Threading.Tasks;
using Wormhole.DomainModel;

namespace Wormhole.Interface
{
    public interface ITenantLogic
    {
        Task<AddTenantOutput> AddTenant(Tenant tenant);
        Task<EditTenantOutput> EditTenant(Tenant tenant);
        Task<DeleteTenantOutput> DeleteTenant();
    }
}

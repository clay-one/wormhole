using System.Collections.Generic;
using System.Threading.Tasks;
using Wormhole.DomainModel;
using Wormhole.DTO;

namespace Wormhole.DataImplementation
{
    public interface ITenantDa
    {
        Task<List<Tenant>> FindTenants();
        Task<AddTenantOutput> AddTenant(Tenant tenant);
    }
}
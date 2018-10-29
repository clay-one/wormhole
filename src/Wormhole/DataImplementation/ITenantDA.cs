using System.Collections.Generic;
using System.Threading.Tasks;
using Wormhole.DomainModel;

namespace Wormhole.DataImplementation
{
    public interface ITenantDa
    {
        Task<List<Tenant>> FindTenants();
        Task AddTenant(Tenant tenant);

    }
}
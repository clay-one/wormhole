using System.Collections.Generic;
using System.Threading.Tasks;
using Wormhole.DomainModel;

namespace Wormhole
{
    public interface ITenantDA
    {
        Task<List<Tenant>> FindTenants();
        Task AddTenant(Tenant tenant);

    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using Wormhole.DomainModel;

namespace Wormhole
{
    public interface ITenantDA
    {
        Task<IList<Tenant>> FindTenants();

    }
}
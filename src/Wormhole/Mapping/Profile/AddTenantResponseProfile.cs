using Wormhole.Api.Model;
using Wormhole.DomainModel;

namespace Wormhole.Mapping.Profile
{
    public class AddTenantResponseProfile : global::AutoMapper.Profile
    {
        public AddTenantResponseProfile()
        {
            CreateMap<Tenant, AddTenantResponse>();
        }
    }
}



using Wormhole.Api.Model;
using Wormhole.DomainModel;

namespace Wormhole.Mapping.Profile
{
    public class AddTenantRequestProfile : global::AutoMapper.Profile
    {
        public AddTenantRequestProfile()
        {
            CreateMap<AddTenantRequest, Tenant>();
        }
    }
}

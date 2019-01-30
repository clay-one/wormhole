using Wormhole.Api.Model.Tenant;
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

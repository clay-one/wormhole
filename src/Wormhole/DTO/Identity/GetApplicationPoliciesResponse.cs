using System.Collections.Generic;

namespace Wormhole.DTO.Identity
{
    public class GetApplicationPoliciesResult
    {
        public IList<Policy> Policies { get; set; }
        public string TenantIdentifier { get; set; }
    }
}


using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Wormhole.DTO.Identity;
using Wormhole.Logic;

namespace Wormhole.Api.Cache
{
    public class IdentityPolicyCache
    {
        private readonly IMemoryCache _cache;
        private readonly IdentityLogic _identityLogic;

        public IdentityPolicyCache(IMemoryCache cache, IdentityLogic identityLogic, ILogger<IdentityPolicyCache> logger)
        {
            _cache = cache;
            _identityLogic = identityLogic;
            Logger = logger;
        }

        private ILogger<IdentityPolicyCache> Logger { get; }

        public async Task<ApplicationInformation> GetApplicationInformationAsync(string appId)
        {
            var getValueResult = _cache.TryGetValue(appId, out var policies);

            if (getValueResult)
                return policies as ApplicationInformation;

            var getApplicationPoliciesResult = await _identityLogic.GetApplicationPoliciesAsync(appId);

            Logger.LogDebug(
                $"IdentityPolicyCache_GetApplicationInformationAsync: Calling identity logic result: {getApplicationPoliciesResult}");

            var applicationInformation = new ApplicationInformation
            {
                Policies = getApplicationPoliciesResult.Result.Policies,
                TenantIdentifier = getApplicationPoliciesResult.Result.TenantIdentifier
            };

            return _cache.Set(appId, applicationInformation, TimeSpan.FromHours(1));
        }
    }

    public class ApplicationInformation
    {
        public IList<Policy> Policies { get; set; }
        public string TenantIdentifier { get; set; }
    }
}
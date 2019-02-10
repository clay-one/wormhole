using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Wormhole.Api.Cache;

namespace Wormhole.Api.Attributes
{
    public class ClientAuthenticationAttribute : ActionFilterAttribute
    {
        private const string IdentityAppIdKey = "Appson-Identity-App-Id";
        private const string ClientCertKey = "X-SSL-Client-Cert";
        private const string ClientVerifyKey = "X-SSL-Client-Verify";
        private const string ClientSh1Key = "X-SSL-Client-SHA1";
        private const string ClientCnKey = "X-SSL-Client-CN";

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var httpContext = context.HttpContext;

            if (await IsClientAuthenticated(httpContext))
            {
                await next();
            }

            else
            {
                httpContext.Response.StatusCode = 403;
                await httpContext.Response.WriteAsync("Application is not authenticated.");
            }
        }

        private async Task<bool> IsClientAuthenticated(HttpContext httpContext)
        {
            try
            {
                var appIdExists = httpContext.Request.Headers.Keys.Contains(IdentityAppIdKey);

                if (!appIdExists) return false;

                var appId = httpContext.Request.Headers[IdentityAppIdKey];

                var identityPolicyCache = httpContext.RequestServices.GetService<IdentityPolicyCache>();

                var appInfo = await identityPolicyCache.GetApplicationInformationAsync(appId);

                if (appInfo?.Policies == null || !appInfo.Policies.Any())
                    return true;

                var commonName = appInfo.Policies.FirstOrDefault()?.Certificate.CommonName;

                var isCertificateValid = true;

                if (!string.IsNullOrWhiteSpace(commonName))
                    isCertificateValid = IsCertificateValid(commonName, httpContext);

                return isCertificateValid;
            }

            catch (Exception)
            {
                return false;
            }
        }

        private bool IsCertificateValid(string commonName, HttpContext httpContext)
        {
            var headers = httpContext.Request.Headers;

            if (headers.Keys.Contains(ClientCertKey) && headers.Keys.Contains(ClientVerifyKey) &&
                headers.Keys.Contains(ClientSh1Key) && headers.Keys.Contains(ClientCnKey))
            {
                var clientCertValue = headers[ClientCertKey];
                var clientVerifyValue = headers[ClientVerifyKey];
                var clientSh1Value = headers[ClientSh1Key];
                var clientCnValue = headers[ClientCnKey];

                if (clientCertValue == "1" && clientVerifyValue == "0" && !string.IsNullOrWhiteSpace(clientCnValue))
                {
                    var isCertificateValid =
                        commonName.Equals(clientCnValue, StringComparison.InvariantCultureIgnoreCase);

                    return isCertificateValid;
                }

                return false;
            }
            return false;
        }
    }
}
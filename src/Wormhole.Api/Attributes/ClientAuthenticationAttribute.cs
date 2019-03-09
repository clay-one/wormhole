using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var httpContext = context.HttpContext;

            if (await IsClientAuthenticated(httpContext))
            {
                await next();
            }

            else
            {
                context.HttpContext.Response.StatusCode = 403;
                await context.HttpContext.Response.WriteAsync("Application is not authenticated.");
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
            var certificate = httpContext.Connection.ClientCertificate;

            if (certificate == null)
                return false;

            var contextCertcommonName = certificate.GetNameInfo(X509NameType.SimpleName, false);

            var isCertificateValid =
                commonName.Equals(contextCertcommonName, StringComparison.InvariantCultureIgnoreCase);

            return isCertificateValid;
        }
    }
}
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Wormhole.DTO.Identity;

namespace Wormhole.Logic
{
    public class IdentityLogic
    {
        private readonly HttpClient _httpClient;

        public IdentityLogic(ILogger<IdentityLogic> logger)
        {
            Logger = logger;
            _httpClient = new HttpClient
            {
                DefaultRequestHeaders = { { "Appson-Identity-App-Id", "494AE7D7-BEB2-4490-A493-6ED9CFFE4AB3" } }
            };
        }

        private ILogger<IdentityLogic> Logger { get; }

        public async Task<IdentityResponse> GetApplicationPoliciesAsync(string appId)
        {
            try
            {
                var apiResponse =
                    await _httpClient.GetAsync($"https://accounts.appson.ir/api/application/policies/{appId}");

                Logger.LogDebug($"IdentityLogic_GetApplicationPoliciesAsync: Identity response: {apiResponse}");

                var stringifiedResult = await apiResponse.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<IdentityResponse>(stringifiedResult);
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
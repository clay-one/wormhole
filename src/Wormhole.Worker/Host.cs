using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Wormhole.Worker
{
    internal class Host
    {
        public static async Task Main(string[] args)
        {
            var host = new WormholeHostBuilder().BuildHost(args);
            var logger = host.Services.GetService<ILogger<Host>>();
            try
            {
                await host.StartAsync();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Exception in worker application, ");
                throw;
            }
        }
    }
}
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Wormhole.InputChannels.Kafka.Consumer
{
    internal class Host
    {
        public static void Main(string[] args)
        {
            var host = new WormholeHostBuilder().BuildHost(args);
            var logger = host.Services.GetService<ILogger<Host>>();
            try
            {
                host.StartAsync().GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Exception in kafka input channel application, ");
                throw;
            }
        }
    }
}
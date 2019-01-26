using System.Threading.Tasks;

namespace Wormhole.Worker
{
    internal class Host
    {
        public static async Task Main(string[] args)
        {
            var host = new WormholeHostBuilder().BuildHost(args);
            await host.StartAsync();
        }
    }
}
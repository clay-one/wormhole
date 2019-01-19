using System.Threading.Tasks;
using Nebula;

namespace Wormhole.Worker
{
    internal class Host
    {
        //private static readonly NebulaContext NebulaContext = new NebulaContext();

        public static async Task Main(string[] args)
        {
            var host = new WormholeHostBuilder().BuildHost(args);
            await host.StartAsync();
        }
    }
}
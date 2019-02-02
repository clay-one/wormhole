namespace Wormhole.InputChannels.Kafka.Consumer
{
    internal class Host
    {
        public static void Main(string[] args)
        {
            var host = new WormholeHostBuilder().BuildHost(args);
            host.StartAsync().GetAwaiter().GetResult();
        }
    }
}
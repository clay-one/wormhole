using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Wormhole.Worker;

namespace Wormhole.Integration.Tests
{
    public class TestHostBuilder : WormholeHostBuilder
    {
        public override void ConfigureLogging(HostBuilderContext hostContext, ILoggingBuilder builder)
        {
        }
        
    }
}
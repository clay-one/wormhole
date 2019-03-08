using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Wormhole.Integration.Tests.Base
{
    public class TestFixture : IDisposable
    {
        private readonly IHost _host;

        public TestFixture()
        {
            _host = new TestHostBuilder().BuildHost(null);
            ServiceProvider = _host.Services;
        }

        public IServiceProvider ServiceProvider { get; }


        public void Dispose()
        {
            _host.StopAsync().GetAwaiter().GetResult();
            _host.Dispose();
        }

        public async Task Start()
        {
            await _host.StartAsync();
        }
    }
}
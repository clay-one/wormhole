using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using NLog.Web;

namespace Wormhole.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseKestrel((context, options) =>
                {
                    options.Configure(context.Configuration.GetSection("Kestrel"));
                    options.ConfigureHttpsDefaults(httpConfigOptions =>
                        {
                            httpConfigOptions.ClientCertificateMode = ClientCertificateMode.AllowCertificate;
                        });
                })
                .UseNLog();
        }
    }
}
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;

namespace Wormhole.Worker
{
    internal class Host
    {
        public static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureHostConfiguration(hostConfigBuilder =>
                {
                    hostConfigBuilder.SetBasePath(Directory.GetCurrentDirectory());
                    hostConfigBuilder.AddJsonFile("hostsettings.json", true);
                    hostConfigBuilder.AddCommandLine(args);
                })
                .ConfigureAppConfiguration((hostContext, appConfigBuilder) =>
                {
                    appConfigBuilder.AddJsonFile("appsettings.json", optional: true);
                    appConfigBuilder.AddJsonFile(
                        $"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json",
                        optional: true);
                    appConfigBuilder.AddCommandLine(args);
                })
                .ConfigureLogging((hostContext, builder) =>
                {
                    if (hostContext.HostingEnvironment.IsDevelopment())
                    {
                        builder.AddConsole();
                    }
                    builder.AddConfiguration(hostContext.Configuration.GetSection("Logging"));

                    var nlogConfigFile = GetNlogConfigFileName(hostContext);
                    LogManager.Configuration = new XmlLoggingConfiguration(nlogConfigFile);

                    builder.AddNLog(new NLogProviderOptions());

                })
                .UseConsoleLifetime()
                .Build();


            await host.StartAsync();
        }

        private static string GetNlogConfigFileName(HostBuilderContext hostContext)
        {
            const string rootConfigFile = "nlog.config";
            var configFile = rootConfigFile;
            var envSpecificConfigFile = $"nlog.{hostContext.HostingEnvironment.EnvironmentName}.config";
            if (File.Exists(envSpecificConfigFile))
            {
                configFile = envSpecificConfigFile;
            }

            return configFile;
        }
    }
}
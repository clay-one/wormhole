using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using Wormhole.Configurations;
using Wormhole.Interface;
using Wormhole.Logic;
using Wormhole.Utils;

namespace Wormhole.InputChannels.Kafka.Consumer
{
    public class WormholeHostBuilder : HostBuilder
    {
        public IHost BuildHost(string[] args)
        {
            return ConfigureHostConfiguration(hostConfigBuilder =>
                {
                    ConfigureHostConfiguration(args, hostConfigBuilder);
                })
                .ConfigureAppConfiguration((hostContext, appConfigBuilder) =>
                {
                    ConfigureAppConfiguration(args, appConfigBuilder, hostContext);
                })
                .ConfigureLogging(ConfigureLogging)
                .ConfigureServices(ConfigureServices)
                .UseConsoleLifetime()
                .Build();
        }

        private static void ConfigureHostConfiguration(string[] args, IConfigurationBuilder hostConfigBuilder)
        {
            hostConfigBuilder.SetBasePath(Directory.GetCurrentDirectory());
            hostConfigBuilder.AddJsonFile("hostsettings.json", false);
            if (args != null && args.Length > 0)
            {
                hostConfigBuilder.AddCommandLine(args);
            }
        }

        private static void ConfigureAppConfiguration(string[] args, IConfigurationBuilder appConfigBuilder,
            HostBuilderContext hostContext)
        {
            appConfigBuilder.AddJsonFile("appsettings.json", true);
            appConfigBuilder.AddJsonFile(
                $"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json",
                true);
            if (args != null && args.Length > 0)
            {
                appConfigBuilder.AddCommandLine(args);
            }
        }

        private static void ConfigureLogging(HostBuilderContext hostContext, ILoggingBuilder builder)
        {
            if (hostContext.HostingEnvironment.IsDevelopment())
            {
                builder.AddConsole();
            }

            builder.AddConfiguration(hostContext.Configuration.GetSection("Logging"));

            var nlogConfigFile = GetNlogConfigFileName(hostContext);
            LogManager.Configuration = new XmlLoggingConfiguration(nlogConfigFile);

            builder.AddNLog(new NLogProviderOptions());
        }

        private static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection collection)
        {
            var config = hostContext.Configuration;
            collection
                .AddSingleton<IPublishMessageLogic, PublishMessageLogic>()
                .AddHostedService<ConsumerHostedService>()
                .Configure<KafkaConfig>(config.GetSection(Constants.KafkaConfigSection));
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
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using Wormhole.Configurations;
using Wormhole.DataImplementation;
using Wormhole.Job;
using Wormhole.Utils;

namespace Wormhole.Worker
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

        public virtual void ConfigureHostConfiguration(string[] args, IConfigurationBuilder hostConfigBuilder)
        {
            hostConfigBuilder.SetBasePath(Directory.GetCurrentDirectory());
            hostConfigBuilder.AddJsonFile("hostsettings.json", true);
            if (args != null && args.Length > 0)
            {
                hostConfigBuilder.AddCommandLine(args);
            }
        }

        public virtual void ConfigureAppConfiguration(string[] args, IConfigurationBuilder appConfigBuilder,
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

        public virtual void ConfigureLogging(HostBuilderContext hostContext, ILoggingBuilder builder)
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

        public virtual void ConfigureServices(HostBuilderContext hostContext, IServiceCollection collection)
        {
            var config = hostContext.Configuration;
            collection
                .AddSingleton<IMongoUtil, MongoUtil>()
                .AddSingleton<ITenantDa, TenantDa>()
                .AddSingleton<IOutputChannelDa, OutputChannelDa>()
                .AddSingleton<IMessageLogDa, MessageLogDa>()
                //.AddTransient<IJobProcessor<HttpPushOutgoingQueueStep>, HttpPushOutgoingQueueProcessor>()
                .AddTransient<HttpPushOutgoingQueueProcessor>()
                .AddSingleton<NebulaService>()
                .AddHostedService<ConsumerHostedService>()
                .AddHostedService<EventSubscribtionHostedService>()
                .Configure<KafkaConfig>(config.GetSection(Constants.KafkaConfigSection))
                .Configure<RetryConfiguration>(config.GetSection(Constants.RetryConfigSection))
                .Configure<NebulaConfig>(config.GetSection(Constants.NebulaConfigSection))
                .Configure<ConnectionStringsConfig>(config.GetSection(Constants.ConnectionStringsConfigSection));
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
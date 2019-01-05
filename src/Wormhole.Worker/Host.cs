using System;
using System.IO;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nebula;
using Nebula.Queue;
using Nebula.Queue.Implementation;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using Wormhole.Configurations;
using Wormhole.DataImplementation;
using Wormhole.Interface;
using Wormhole.Job;
using Wormhole.Kafka;
using Wormhole.Logic;
using Wormhole.Utils;

namespace Wormhole.Worker
{
    internal class Host
    {
        //private static readonly NebulaContext NebulaContext = new NebulaContext();

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
                .ConfigureServices((hostContext, collection) =>
                {
                    var config = hostContext.Configuration;
                    collection
                        .AddSingleton<ITenantDa, TenantDa>()
                        .AddSingleton<IOutputChannelDa, OutputChannelDa>()
                        .AddSingleton<IMessageLogDa, MessageLogDa>()
                        
                        //.AddSingleton(NebulaContext)
                        .AddSingleton<NebulaService>() //instead of registering a singleton instance

                        .AddSingleton<IJobProcessor<HttpPushOutgoingQueueStep>, HttpPushOutgoingQueueProcessor>()
                        .AddSingleton<IKafkaProducer, KafkaProducer>()
                        .AddSingleton<IMongoUtil,MongoUtil>()
                        .AddScoped<IPublishMessageLogic, PublishMessageLogic>()
                        .AddTransient<IKafkaConsumer<Null, string>, KafkaConsumer>()

                        //.AddTransient<IConsumerBase, HttpPushOutgoingMessageConsumer>(sp =>
                        //    new HttpPushOutgoingMessageConsumer(sp.GetService<IKafkaConsumer<Null, string>>(),
                        //        sp.GetService<NebulaContext>(),
                        //        sp.GetService<ILoggerFactory>(), ConsumerTopicName))

                        .Configure<KafkaConfig>(config.GetSection(Constants.KafkaConfigSection))
                        .Configure<RetryConfiguration>(config.GetSection(Constants.RetryConfigSection))
                        .Configure<NebulaConfig>(config.GetSection(Constants.NebulaConfigSection))
                        .Configure<ConnectionStringsConfig>(config.GetSection(Constants.ConnectionStringsConfigSection));
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

    public class NebulaService
    {
        private NebulaContext _nebulaContext;
        public NebulaService(IOptions<NebulaConfig> options, IJobProcessor<HttpPushOutgoingQueueStep> jobProcessor)
        {
            var nebulaConfig = options?.Value ?? throw new ArgumentNullException(nameof(options));

            ConfigureNebulaContext(jobProcessor, nebulaConfig);
        }

        private void ConfigureNebulaContext(IJobProcessor<HttpPushOutgoingQueueStep> jobProcessor, NebulaConfig nebulaConfig)
        {
            _nebulaContext = new NebulaContext
            {
                MongoConnectionString = nebulaConfig.MongoConnectionString,
                RedisConnectionString = nebulaConfig.RedisConnectionString
            };

            _nebulaContext.RegisterJobQueue(typeof(DelayedJobQueue<>), QueueType.Delayed);

            _nebulaContext.RegisterJobProcessor(jobProcessor, typeof(HttpPushOutgoingQueueStep));
        }
    }
}
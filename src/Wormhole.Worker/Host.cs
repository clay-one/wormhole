using System.IO;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nebula;
using Nebula.Queue;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
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
        private static readonly NebulaContext NebulaContext = new NebulaContext();

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
                    collection.AddSingleton<ITenantDa, TenantDa>()
                        .AddSingleton<IOutputChannelDa, OutputChannelDa>()
                        .AddSingleton<IMessageLogDa, MessageLogDa>()
                        .AddSingleton(NebulaContext)
                        .AddScoped<IPublishMessageLogic, PublishMessageLogic>()
                        .AddSingleton<IKafkaProducer, KafkaProducer>()
                        .AddTransient<IKafkaConsumer<Null, string>, KafkaConsumer>()
                        //.AddTransient<IConsumerBase, HttpPushOutgoingMessageConsumer>(sp =>
                        //    new HttpPushOutgoingMessageConsumer(sp.GetService<IKafkaConsumer<Null, string>>(),
                        //        sp.GetService<NebulaContext>(),
                        //        sp.GetService<ILoggerFactory>(), ConsumerTopicName))
                        .Configure<KafkaConfig>(hostContext.Configuration.GetSection(Constants.KafkaConfig))
                        .Configure<RetryConfiguration>(
                            hostContext.Configuration.GetSection(Constants.RetryConfiguration))
                        .AddSingleton<IJobProcessor<HttpPushOutgoingQueueStep>, HttpPushOutgoingQueueProcessor>();
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
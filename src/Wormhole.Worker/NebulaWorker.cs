using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nebula;
using Nebula.Queue;
using Nebula.Queue.Implementation;
using Nebula.Storage.Model;
using Newtonsoft.Json;
using NLog;
using NLog.Extensions.Logging;
using Wormhole.Api.Model;
using Wormhole.DataImplementation;
using Wormhole.Interface;
using Wormhole.Job;
using Wormhole.Kafka;
using Wormhole.Logic;
using Wormhole.Utils;

namespace Wormhole.Worker
{
    internal class NebulaWorker
    {
        private static readonly NebulaContext NebulaContext = new NebulaContext();
        private static readonly IConfigurationRoot AppConfiguration = BuildConfiguration(Directory.GetCurrentDirectory());
        private static readonly ServiceProvider ServiceProvider = ConfigureServices();
        private static readonly IDictionary<string, IConsumerBase> Consumers = new ConcurrentDictionary<string, IConsumerBase>();
        private static ILogger<NebulaWorker> Logger { get; set; }
        private static string JobId { get; set; }
        private static string ConsumerTopicName { get; set; }

        public static void Main(string[] args)
        {
            ConfigureLogging();

            ConfigureNebula();
            AppSettingsProvider.MongoConnectionString =
                AppConfiguration.GetConnectionString(Utils.Constants.MongoConnectionString);
            StartNebulaService();
            var topics = GetTopics().GetAwaiter().GetResult();
            JobId = CreateJob().GetAwaiter().GetResult();
            StartConsuming(topics);
        }

        private static async Task<string> CreateJob()
        {
            // todo: static job might be a better choice
            var retryConfig = AppConfiguration.GetSection("RetryConfig");
            var parameters = new OutgoingQueueParameters
            {
                RetryCount = int.Parse(retryConfig.GetChildren().FirstOrDefault(a => a.Key == "Count")?.Value),
                RetryInterval = int.Parse(retryConfig.GetChildren().FirstOrDefault(a => a.Key == "Interval")?.Value)
            };
            var jobId = await NebulaContext.GetJobManager().CreateNewJobOrUpdateDefinition<OutgoingQueueStep>("__none__",
                $"Wormhole-",
                configuration: new JobConfigurationData
                {
                    MaxBatchSize = 1,
                    MaxConcurrentBatchesPerWorker = 5,
                    IdleSecondsToCompletion = 30,
                    MaxBlockedSecondsPerCycle = 60,
                    MaxTargetQueueLength = 100000,
                    Parameters = JsonConvert.SerializeObject(parameters),
                    QueueTypeName = QueueType.Delayed,
                    IsIndefinite = true
                });
            await NebulaContext.GetJobManager().StartJob("__none__", jobId);
            return jobId;
        }

        private static async Task<List<string>> GetTopics()
        {
            var tenantDa = ServiceProvider.GetService<ITenantDa>();
            var topics = await tenantDa.FindTenants();
            return topics.Select(t=>t.Name).ToList();
        }


        private static void StartConsuming(List<string> topics)
        {
            if (topics == null || topics.Count <1)
                throw new Exception("There is no topic for message consumption");

            foreach (var topic in topics)
            {
                var consumer = GetConsumer(topic);
                consumer.Start();
            }
        }

        private static IConsumerBase GetConsumer(string topic)
        {
            ConsumerTopicName = topic;
            var consumer = Consumers.FirstOrDefault(c => c.Key == ConsumerTopicName).Value;
            if (consumer != null) return consumer;
            consumer = ServiceProvider.GetService<IConsumerBase>();
            Consumers.Add(new KeyValuePair<string, IConsumerBase>(ConsumerTopicName, consumer));

            return consumer;
        }


        private static void StartNebulaService()
        {
            if (!Environment.UserInteractive)
            {
                // running as service
                Logger.LogInformation("Windows service starting");
                using (var windowsService = new JobQueueWindowsService(NebulaContext))
                {
                    ServiceBase.Run(windowsService);
                }

                Logger.LogInformation("Windows service started");
            }
            else
            {
                // running as console app

                Console.WriteLine("Wormhole.Worker worker service...");
                NebulaContext.StartWorkerService();
                Console.WriteLine("Service started. Press ENTER to stop.");
            }
        }

        private static void ConfigureLogging()
        {
            ServiceProvider
                .GetService<ILoggerFactory>().AddNLog(new NLogProviderOptions { CaptureMessageTemplates = true, CaptureMessageProperties = true });
            LogManager.LoadConfiguration("nlog.config");


            Logger = ServiceProvider.GetService<ILoggerFactory>()
                .CreateLogger<NebulaWorker>();
            Logger.LogDebug("Starting application");
        }


        private static ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddLogging()
                .AddSingleton<ITenantDa, TenantDa>()
                .AddSingleton(NebulaContext)
                .AddScoped<IPublishMessageLogic, PublishMessageLogic>()
                .AddSingleton<IKafkaProducer, KafkaProducer>()
                .AddTransient<IKafkaConsumer<Null, string>, KafkaConsumer>()
                .AddTransient<IConsumerBase, MessageConsumer>(sp=> new MessageConsumer(sp.GetService<IKafkaConsumer<Null,string>>(),sp.GetService<NebulaContext>(),sp.GetService<ILoggerFactory>(), ConsumerTopicName, JobId))
                .Configure<KafkaConfig>(AppConfiguration.GetSection(Utils.Constants.KafkaConfig))
                .AddSingleton<IFinalizableJobProcessor<OutgoingQueueStep>, OutgoingQueueProcessor>()
                .BuildServiceProvider();
        }

        private static void ConfigureNebula()
        {
            NebulaContext.RegisterJobQueue(typeof(DelayedJobQueue<>), QueueType.Delayed);

            NebulaContext.MongoConnectionString =
                AppConfiguration.GetConnectionString("nebula:mongoConnectionString");
            NebulaContext.RedisConnectionString =
                AppConfiguration.GetConnectionString("nebula:redisConnectionString");
            
            var delayedQueueProcessor = ServiceProvider.GetService<IFinalizableJobProcessor<OutgoingQueueStep>>();
            NebulaContext.RegisterJobProcessor(delayedQueueProcessor, typeof(OutgoingQueueStep));
        }

        private static IConfigurationRoot BuildConfiguration(string path, string environmentName = null)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(path)
                .AddJsonFile("appsettings.json", true, true);

            if (!string.IsNullOrWhiteSpace(environmentName)) builder = builder.AddJsonFile($"appsettings.{environmentName}.json", true);

            builder = builder.AddEnvironmentVariables();
            
            return builder.Build();
        }
    }
}

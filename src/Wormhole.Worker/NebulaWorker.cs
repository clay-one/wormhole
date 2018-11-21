﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using Confluent.Kafka;
using hydrogen.General.Collections;
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
using Wormhole.DataImplementation;
using Wormhole.DomainModel;
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

        private static readonly IConfigurationRoot AppConfiguration =
            BuildConfiguration(Directory.GetCurrentDirectory());

        private static readonly ServiceProvider ServiceProvider = ConfigureServices();

        private static readonly IDictionary<string, IConsumerBase> Consumers =
            new ConcurrentDictionary<string, IConsumerBase>();

        private static readonly IList<OutputChannel> OutputChannels = new List<OutputChannel>();
        private static IConfigurationSection RetryConfig => AppConfiguration.GetSection("RetryConfig");

        private static ILogger<NebulaWorker> Logger { get; set; }

        private static string ConsumerTopicName { get; set; }

        public static void Main(string[] args)
        {
            ConfigureLogging();

            ConfigureNebula();
            AppSettingsProvider.MongoConnectionString =
                AppConfiguration.GetConnectionString(Constants.MongoConnectionString);
            StartNebulaService();
            var topics = GetTopics().GetAwaiter().GetResult();
            CreateJobs().GetAwaiter().GetResult();
            StartConsuming(topics);
        }

        public static IList<string> GetJobIds(string tenant, string category, IList<string> tags)
        {
            if (tags == null || tags.Count < 1)
                return
                    OutputChannels.Where(o => o.FilterCriteria.Category == category && o.TenantId == tenant)
                        .Select(o => o.JobId).ToList();

            return tags.Select(tag => OutputChannels.FirstOrDefault(o =>
                        o.FilterCriteria.Tag == tag && o.FilterCriteria.Category == category && o.TenantId == tenant)
                    ?.JobId)
                .ToList();
        }

        private static async Task CreateJobs()
        {
            OutputChannels.AddAll(await GetOutputChannels());
            await CreateHttpPushOutgingQueueJobsAsync(OutputChannels.Where(o => o.ChannelType == ChannelType.HttpPush)
                .ToList());
        }

        private static async Task CreateHttpPushOutgingQueueJobsAsync(List<OutputChannel> outputChannels)
        {
            var parameters = new HttpPushOutgoingQueueParameters
            {
                RetryCount = int.Parse(RetryConfig.GetChildren().FirstOrDefault(a => a.Key == "Count")?.Value),
                RetryInterval = int.Parse(RetryConfig.GetChildren().FirstOrDefault(a => a.Key == "Interval")?.Value)
            };

            foreach (var outputChannel in outputChannels)
            {
                var channelSpecification = outputChannel.TypeSpecification as HttpPushOutputChannelSpecification;
                if (string.IsNullOrWhiteSpace(outputChannel.JobId))
                {
                    parameters.TargetUrl = channelSpecification.TargetUrl;

                    // todo: static job might be a better choice
                    outputChannel.JobId = await NebulaContext.GetJobManager()
                        .CreateNewJobOrUpdateDefinition<HttpPushOutgoingQueueStep>(
                            "__none__",
                            $"Wormhole_{outputChannel.JobId}",
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
                            }, jobId: $"Wormhole_Job_{outputChannel.Id.ToString()}");
                    var outputChannelDa = ServiceProvider.GetService<IOutputChannelDa>();
                    await outputChannelDa.SetJobId(outputChannel.Id.ToString(), outputChannel.JobId);
                    await NebulaContext.GetJobManager().StartJob("__none__", outputChannel.JobId);
                }
            }
        }

        private static async Task<List<string>> GetTopics()
        {
            var tenantDa = ServiceProvider.GetService<ITenantDa>();
            var topics = await tenantDa.FindTenants();
            return topics.Select(t => t.Name).ToList();
        }

        private static async Task<List<OutputChannel>> GetOutputChannels()
        {
            var outputChannelDa = ServiceProvider.GetService<IOutputChannelDa>();
            return await outputChannelDa.FindOutputChannels();
        }


        private static void StartConsuming(List<string> topics)
        {
            if (topics == null || topics.Count < 1)
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
                .GetService<ILoggerFactory>()
                .AddNLog(new NLogProviderOptions {CaptureMessageTemplates = true, CaptureMessageProperties = true});
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
                .AddSingleton<IOutputChannelDa, OutputChannelDa>()
                .AddSingleton(NebulaContext)
                .AddScoped<IPublishMessageLogic, PublishMessageLogic>()
                .AddSingleton<IKafkaProducer, KafkaProducer>()
                .AddTransient<IKafkaConsumer<Null, string>, KafkaConsumer>()
                .AddTransient<IConsumerBase, MessageConsumer>(sp =>
                    new MessageConsumer(sp.GetService<IKafkaConsumer<Null, string>>(), sp.GetService<NebulaContext>(),
                        sp.GetService<ILoggerFactory>(), ConsumerTopicName))
                .Configure<KafkaConfig>(AppConfiguration.GetSection(Constants.KafkaConfig))
                .AddSingleton<IFinalizableJobProcessor<HttpPushOutgoingQueueStep>, HttpPushOutgoingQueueProcessor>()
                .BuildServiceProvider();
        }

        private static void ConfigureNebula()
        {
            NebulaContext.RegisterJobQueue(typeof(DelayedJobQueue<>), QueueType.Delayed);

            NebulaContext.MongoConnectionString =
                AppConfiguration.GetConnectionString("nebula:mongoConnectionString");
            NebulaContext.RedisConnectionString =
                AppConfiguration.GetConnectionString("nebula:redisConnectionString");

            var httpPushdelayedQueueProcessor =
                ServiceProvider.GetService<IFinalizableJobProcessor<HttpPushOutgoingQueueStep>>();
            NebulaContext.RegisterJobProcessor(httpPushdelayedQueueProcessor, typeof(HttpPushOutgoingQueueStep));
        }

        private static IConfigurationRoot BuildConfiguration(string path, string environmentName = null)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(path)
                .AddJsonFile("appsettings.json", true, true);

            if (!string.IsNullOrWhiteSpace(environmentName))
                builder = builder.AddJsonFile($"appsettings.{environmentName}.json", true);

            builder = builder.AddEnvironmentVariables();

            return builder.Build();
        }
    }
}
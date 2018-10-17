using System;
using System.IO;
using System.ServiceProcess;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nebula;
using Nebula.Queue;
using Nebula.Queue.Implementation;
using Wormhole.DataImplementation;
using Wormhole.Job;

namespace Wormhole.Worker
{
    internal class Program
    {
        private static readonly NebulaContext NebulaContext = new NebulaContext();
        private static readonly IConfigurationRoot AppConfiguration = BuildConfiguration(Directory.GetCurrentDirectory());
        private static readonly ServiceProvider ServiceProvider = ConfigureServices();
        private static ILogger<Program> Logger { get; set; }

        private static void Main(string[] args)
        {
            ConfigureLogging();

            ConfigureNebula();

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
                Console.ReadLine();

                Console.WriteLine("Stopping the serivce...");
                NebulaContext.StopWorkerService();
                Console.WriteLine("Service stopped, everything looks clean.");
            }
            
        }

        private static void ConfigureLogging()
        {
            ServiceProvider
                .GetService<ILoggerFactory>()
                .AddConsole(LogLevel.Debug);

            Logger = ServiceProvider.GetService<ILoggerFactory>()
                .CreateLogger<Program>();
            Logger.LogDebug("Starting application");
        }


        private static ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddLogging()
                .AddSingleton<ITenantDA, TenantDA>()
                .AddSingleton<IFinalizableJobProcessor<OutgoingQueueStep>, OutgoingQueueProcessor>()
                .BuildServiceProvider();
        }

        private static void ConfigureNebula()
        {
            NebulaContext.RegisterJobQueue(typeof(DelayedJobQueue<>), QueueType.Delayed);

            NebulaContext.MongoConnectionString =
                AppConfiguration.GetConnectionString("mongoConnectionString");
            NebulaContext.RedisConnectionString =
                AppConfiguration.GetConnectionString("redisConnectionString");
            
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

using System;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using ComposerCore;
using ComposerCore.Implementation;
using ComposerCore.Utility;
using log4net;
using log4net.Config;
using Microsoft.Extensions.Configuration;
using Nebula;
using Nebula.Queue;
using Nebula.Queue.Implementation;
using Wormhole.Job;

namespace Wormhole.Worker
{
    internal class Program
    {
        public static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly NebulaContext NebulaContext = new NebulaContext();
        private static readonly IConfigurationRoot AppConfiguration = BuildConfiguration(Directory.GetCurrentDirectory());

        private static IComponentContext Composer { get; set; }


        private static void Main(string[] args)
        {
            XmlConfigurator.Configure(LogManager.GetRepository(Assembly.GetCallingAssembly()));
            ConfigureComposer();
            ConfigureNebula();

            if (!Environment.UserInteractive)
            {
                // running as service
                Log.Info("Windows service starting");
                using (var windowsService = new JobQueueWindowsService(NebulaContext))
                {
                    ServiceBase.Run(windowsService);
                }

                Log.Info("Windows service started");
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

        private static void ConfigureComposer()
        {
            Composer = new ComponentContext();
            Composer.RegisterAssembly("Nebula");
            Composer.RegisterAssembly("Wormhole");
        }

        private static void ConfigureNebula()
        {
            NebulaContext.RegisterJobQueue(typeof(DelayedJobQueue<>), QueueType.Delayed);

            NebulaContext.MongoConnectionString =
                AppConfiguration.GetConnectionString("mongoConnectionString");
            NebulaContext.RedisConnectionString =
                AppConfiguration.GetConnectionString("redisConnectionString");

            var delayedQueueProcessor = Composer.GetComponent(typeof(IFinalizableJobProcessor<OutgoingQueueStep>));
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

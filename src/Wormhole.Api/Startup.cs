using System;
using System.IO;
using System.Linq;
using log4net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wormhole.Api.Configuration;
using Wormhole.DataImplementation;
using Wormhole.DataImplementation.Configuration;
using Wormhole.Interface;
using Wormhole.Kafka;
using Wormhole.Logic;
using Wormhole.Utils;

namespace Wormhole.Api
{
    public class Startup
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Startup));

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddScoped<IOutputChannelDa, OutputChannelDa>();
            services.AddScoped<IInputChannelDa, InputChannelDa>();
            services.AddScoped<IPublishMessageLogic, PublishMessageLogic>();
            services.AddScoped<ITenantLogic, TenantLogic>();
            services.AddScoped<ITenantDa, TenantDa>();
            services.AddSingleton<IKafkaProducer, KafkaProducer>();
            ConfigureAppSettingObjects(services);
        }

        private void ConfigureMongoConfigurationObjects()
        {
            var interfaceType = typeof(IMongoCollectionConfig);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => interfaceType.IsAssignableFrom(p) && p.IsClass);
            foreach (var type in types)
            {
                var config = (IMongoCollectionConfig) Activator.CreateInstance(type);
                config.Configure();
            }
        }

        private void ConfigureAppSettingObjects(IServiceCollection services)
        {
            services.Configure<KafkaConfig>(Configuration.GetSection(Constants.KafkaConfig));
            AppSettingsProvider.MongoConnectionString =
                Configuration.GetConnectionString(Constants.MongoConnectionString);
            services.Configure<ClientConfig>(Configuration.GetSection(Constants.ClientConfig));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            ConfigureLog4Net(env.ContentRootPath,loggerFactory);
            ConfigureTypeMappings();
            ConfigureMongoConfigurationObjects();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            MapWebApi(app);
            app.UseHttpsRedirection();
        }

        private void ConfigureTypeMappings()
        {
            AutoMapperConfig.ConfigureAllMappers();
        }

        private void ConfigureLog4Net(string contentRootPath, ILoggerFactory loggerFactory)
        {
            var rootLogFolder = Configuration.GetSection("logging").GetChildren().FirstOrDefault(a => a.Key == "RootLogFolder")?.Value;
            if (string.IsNullOrWhiteSpace(rootLogFolder))
            {
                var appRootPath = contentRootPath;
                if (appRootPath == null)
                    throw new InvalidOperationException(
                        "Cannot configure log4net because HostingEnvironment.MapPath returns null");

                rootLogFolder = Path.GetFullPath(Path.Combine(appRootPath, "..\\log"));
            }
            GlobalContext.Properties["RootLogFolder"] = rootLogFolder;

            loggerFactory.AddLog4Net();
            Log.Info("Application started");
        }


        private static void MapWebApi(IApplicationBuilder app)
        {
            app.Map("/api", apiApp => { apiApp.UseMvc(); });
        }
    }
}

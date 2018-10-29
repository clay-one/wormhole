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
using Wormhole.DataImplementation;
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
            services.AddScoped<IOutputChannelDA, OutputChannelDA>();
            services.AddScoped<IPublishMessageLogic, PublishMessageLogic>();
            services.AddSingleton<IKafkaProducer, KafkaProducer>();
            ConfigureAppSettingObjects(services);
        }

        private void ConfigureAppSettingObjects(IServiceCollection services)
        {
            services.Configure<KafkaConfig>(Configuration.GetSection(Constants.KafkaConfig));
            AppSettingsProvider.MongoConnectionString =
                Configuration.GetConnectionString(Constants.MongoConnectionString);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            ConfigureLog4Net(env.ContentRootPath,loggerFactory);
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

using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NLog.Web;
using Wormhole.Api.Configuration;
using Wormhole.DataImplementation;
using Wormhole.DataImplementation.Configuration;
using Wormhole.Interface;
using Wormhole.Kafka;
using Wormhole.Logic;
using Wormhole.Utils;
using Swashbuckle.AspNetCore.Swagger;

namespace Wormhole.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            env.ConfigureNLog("nlog.config");
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddScoped<IOutputChannelDa, OutputChannelDa>();
            services.AddScoped<IInputChannelDa, InputChannelDa>();
            services.AddScoped<IPublishMessageLogic, PublishMessageLogic>();
            services.AddScoped<ITenantLogic, TenantLogic>();
            services.AddScoped<ITenantDa, TenantDa>();
            services.AddScoped<IMessageLogDa, MessageLogDa>();
            services.AddSingleton<IKafkaProducer, KafkaProducer>();
            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new Info {Title = "Wormhole Apis", Version = "v1"}); });
            ConfigureAppSettingObjects(services);
        }

        private static void ConfigureMongoConfigurationObjects()
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
            services.Configure<KafkaConfig>(Configuration.GetSection(Constants.KafkaConfigSection));
            AppSettingsProvider.MongoConnectionString =
                Configuration.GetConnectionString(Constants.MongoConnectionStringSection);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            ConfigureLogging(app, loggerFactory);
            ConfigureTypeMappings();
            ConfigureMongoConfigurationObjects();
            ConfigureSwagger(app);

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

        private void ConfigureSwagger(IApplicationBuilder app)
        {
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
                c.RoutePrefix = string.Empty;
            });
        }

        private void ConfigureTypeMappings()
        {
            AutoMapperConfig.ConfigureAllMappers();
        }

        private void ConfigureLogging(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            //add NLog to ASP.NET Core
            loggerFactory.AddNLog();
        }

        private static void MapWebApi(IApplicationBuilder app)
        {
            app.Map("/api", apiApp => { apiApp.UseMvc(); });
        }
    }
}
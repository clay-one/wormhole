using System;
using System.Collections.Generic;
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
using Wormhole.Configurations;

namespace Wormhole.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            _configuration = configuration;

            env.ConfigureNLog($"nlog.{env.EnvironmentName}.config");     
        }

        private readonly IConfiguration _configuration;

        // This method gets called by the runtime. Use this method to add services to the container.
        public virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddScoped<IOutputChannelDa, OutputChannelDa>();
            services.AddScoped<IInputChannelDa, InputChannelDa>();
            services.AddScoped<ITenantDa, TenantDa>();
            services.AddScoped<IMessageLogDa, MessageLogDa>();
            services.AddScoped<IPublishMessageLogic, PublishMessageLogic>();
            services.AddScoped<ITenantLogic, TenantLogic>();
            services.AddSingleton<IMongoUtil, MongoUtil>();
            services.AddSingleton<IMongoCollectionConfig,InputChannelConfig>();
            services.AddSingleton<IMongoCollectionConfig,OutputChannelConfig>();
            services.AddSingleton<IMongoCollectionConfig,TenantConfig>();
            services.AddSingleton<IKafkaProducer, KafkaProducer>();
            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new Info {Title = "Wormhole Apis", Version = "v1"}); });
            ConfigureAppSettingObjects(services);
        }

        private static void ConfigureMongo(IApplicationBuilder applicationBuilder)
        {
            //var interfaceType = typeof(IMongoCollectionConfig);
            //var types = AppDomain.CurrentDomain.GetAssemblies()
            //    .SelectMany(s => s.GetTypes())
            //    .Where(p => interfaceType.IsAssignableFrom(p) && p.IsClass);
            //foreach (var type in types)
            //{
            //    var config = (IMongoCollectionConfig) Activator.CreateInstance(type);
            //    config.Configure();
            //}
            var collectionConfigs = applicationBuilder.ApplicationServices.GetServices<IMongoCollectionConfig>();
            foreach (var config in collectionConfigs)
            {
                config.Configure();
            }
        }

        private void ConfigureAppSettingObjects(IServiceCollection services)
        {
            services.Configure<ConnectionStringsConfig>(_configuration.GetSection(Constants.ConnectionStringsConfigSection));
            services.Configure<KafkaConfig>(_configuration.GetSection(Constants.KafkaConfigSection));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            ConfigureLogging(app, loggerFactory);
            ConfigureTypeMappings();
            ConfigureMongo(app);
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
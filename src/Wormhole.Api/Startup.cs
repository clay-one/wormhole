using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wormhole.Kafka;
using Wormhole.Logic;

namespace Wormhole.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddScoped<IPublishMessageLogic, PublishMessageLogic>();
            services.AddSingleton<IKafkaProducer, KafkaProducer>();
            services.Configure<KafkaConfig>(Configuration.GetSection("KafkaConfig"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
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
        

        private static void MapWebApi(IApplicationBuilder app)
        {
            app.Map("/api", apiApp => { apiApp.UseMvc(); });
        }
    }
}

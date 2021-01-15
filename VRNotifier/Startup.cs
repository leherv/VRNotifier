using System;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VRNotifier.Config;
using VRNotifier.Services;
using VRPersistence.Config;

namespace VRNotifier
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
            services.AddControllers();
            
            services.Configure<TrackedMediaSettings>(Configuration.GetSection(nameof(TrackedMediaSettings)));
            services.Configure<DiscordSettings>(Configuration.GetSection($"NotificationSettings:{nameof(DiscordSettings)}"));
            
            services.AddSingleton<DiscordSocketClient>();
            services.AddSingleton<CommandService>();
            services.AddSingleton<CommandHandlingService>();
            services.AddSingleton<DiscordService>();
            services.AddHostedService(provider => provider.GetRequiredService<DiscordService>());
            
            var vrPersistenceClientSettings = Configuration
                .GetSection(nameof(VRPersistenceClientSettings))
                .Get<VRPersistenceClientSettings>();
            services.AddHttpClient<IVRPersistenceClient, VRPersistenceClient>(selfServiceClient =>
            {
                selfServiceClient.BaseAddress = new Uri(vrPersistenceClientSettings.Endpoint);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
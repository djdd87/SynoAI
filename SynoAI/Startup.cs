using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using SynoAI.Services;
using System.Threading.Tasks;
using SynoAI.Hubs;
using System.Collections.Generic;
using System.Linq;

namespace SynoAI
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IAIService, AIService>();
            services.AddScoped<ISynologyService, SynologyService>();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SynoAI", Version = "v1" });
            });

            services.AddRazorPages();

            // euquiq: Needed for realtime update from each camera valid snapshot into client's web browser
            services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IConfiguration configuration, IHostApplicationLifetime lifetime, ILogger<Startup> logger, ISynologyService synologyService)
        {
            Config.Generate(logger, configuration);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SynoAI v1"));
            }
            
            // euquiq: Allows /wwwroot's static files (mainly our Javascript code for RT monitoring the cameras)
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                // euquiq: Used by SignalR to contact each online client with snapshot updates
                endpoints.MapHub<SynoAIHub>("/synoaiHub");

                // euquiq: Web interface mapped inside HomeController.cs
                endpoints.MapControllers();   
            });

            lifetime.ApplicationStarted.Register(() =>
            {
                List<Task> initializationTasks = new List<Task>();
                initializationTasks.Add(synologyService.InitialiseAsync());
                initializationTasks.AddRange(Config.Notifiers.Select(n => n.InitializeAsync(logger)));
                Task.WhenAll(initializationTasks).Wait();
            });

            lifetime.ApplicationStopping.Register(() =>
            {
                List<Task> cleanupTasks = new List<Task>();
                cleanupTasks.AddRange(Config.Notifiers.Select(n => n.CleanupAsync(logger)));
                Task.WhenAll(cleanupTasks).Wait();
            });
        }
    }
}

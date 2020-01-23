// <copyright file="Startup.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration
{
    using System.Net;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.Celebration.Authentication;
    using Microsoft.Teams.Apps.Celebration.Bot;
    using Microsoft.Teams.Apps.Celebration.Common.BFS;
    using Microsoft.Teams.Apps.Celebration.Common.Core;
    using Microsoft.Teams.Apps.Celebration.Common.Queues;
    using Microsoft.Teams.Apps.Celebration.Common.Repositories;
    using Microsoft.Teams.Apps.Celebration.Common.Validations;

    /// <summary>
    /// Register services in DI container, and set up middle-wares in the pipeline.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">IConfiguration instance.</param>
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        /// <summary>
        /// Gets the IConfiguration instance.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">IServiceCollection instance.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry();

            services.AddControllersWithViews();

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });

            // Initialize the Celebrations application's configuration settings.
            var configurationSettings = new ConfigurationSettings();
            this.Configuration.Bind(configurationSettings);
            services.AddSingleton(configurationSettings);

            services.AddCelebrationsAuthentication(this.Configuration);
            services.AddCelebrationsBot();
            services.AddCelebrationsRepositories();
            services.AddCelebrationsMessageQueue();
            services.AddCelebrationsBFS(configurationSettings);
            services.AddCelebrationsValidations();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">IApplicationBuilder instance, which is a class that provides the mechanisms to configure an application's request pipeline.</param>
        /// <param name="env">IHostingEnvironment instance, which provides information about the web hosting environment an application is running in.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseExceptionHandler(applicationBuilder => this.HandleGlobalException(applicationBuilder));
            app.UseHsts();

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
        }

        private void HandleGlobalException(IApplicationBuilder applicationBuilder)
        {
            applicationBuilder.Run(async context =>
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                if (contextFeature != null)
                {
                    var loggerFactory = applicationBuilder.ApplicationServices.GetService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger(nameof(Startup));
                    logger.LogError($"{contextFeature.Error}");

                    await context.Response.WriteAsync(new
                    {
                        context.Response.StatusCode,
                        Message = "Internal Server Error.",
                    }.ToString());
                }
            });
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ChatApp.Services;
using ForEvolve.NETCore.Azure.Storage.Table;
using ChatApp.Models;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace ChatApp
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                builder.AddUserSecrets<Startup>();
            }
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        private TelemetryClient Telemetry { get; } = new TelemetryClient();

        public string ApiName { get; } = "Prog3 2017 - ChatApp API";

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add ChatApp services
            services.AddSingleton(new TableStorageRepository<ChatEntryEntity>(new TableStorageSettings
            {
                AccountKey = Configuration.GetValue<string>("ChatAppStorage:AccountKey"),
                AccountName = Configuration.GetValue<string>("ChatAppStorage:AccountName"),
                TableName = Configuration.GetValue<string>("ChatAppStorage:TableName"),
            }));
            services.AddSingleton<IChatService, ChatService>();
            services.AddSingleton(Telemetry);

            // Add framework services.
            services.AddMvc();

            // Add swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = ApiName, Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            // Redirect users to SwaggerUI when they it the application root
            app.MapWhen(context =>
            {
                var isPathBaseNull = string.IsNullOrWhiteSpace(context.Request.PathBase);
                var isEmptyAndSlashPath = (isPathBaseNull && context.Request.Path == "/");
                var result = isEmptyAndSlashPath || context.Request.PathBase.Equals(context.Request.Path);
                Telemetry.TrackTrace($"RootRedirect-isPathBaseNull: {isPathBaseNull}", SeverityLevel.Information);
                Telemetry.TrackTrace($"RootRedirect-isEmptyAndSlashPath: {isEmptyAndSlashPath}", SeverityLevel.Information);
                Telemetry.TrackTrace($"RootRedirect-PathBase: {context.Request.PathBase}", SeverityLevel.Information);
                Telemetry.TrackTrace($"RootRedirect-Path: {context.Request.Path}", SeverityLevel.Information);
                Telemetry.TrackTrace($"RootRedirect-result: {result}", SeverityLevel.Information);
                return result;
            }, 
            a =>
            {
                a.Run(context =>
                {
                    context.Response.Redirect("/swagger/");
                    return Task.FromResult(0);
                });
            });

            // MVC
            app.UseMvc();

            // Connect Swagger & SwaggerUI
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", ApiName);
            });
        }
    }
}

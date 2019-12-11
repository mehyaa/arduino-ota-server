using Mehyaa.Arduino.OTAServer.Data.Contexts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace Mehyaa.Arduino.OTAServer.Web
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<OTAContext>(
                options =>
                    options.UseSqlite(_configuration.GetConnectionString(nameof(OTAContext))));

            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IHostApplicationLifetime hostApplicationLifetime)
        {
            app.UseExceptionHandler(
                builder =>
                    builder.Run(
                        async context =>
                        {
                            var logger = context.RequestServices.GetService<ILogger<Startup>>();

                            var webHostEnvironment = context.RequestServices.GetService<IWebHostEnvironment>();

                            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                            context.Response.ContentType = "text/plain";

                            var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();

                            if (exceptionHandlerFeature != null)
                            {
                                var exception = exceptionHandlerFeature.Error;

                                logger.LogError(exception, $"{context.TraceIdentifier}: {exception.Message}");

                                if (webHostEnvironment.IsDevelopment())
                                {
                                    await context.Response.WriteAsync(exception.ToString()).ConfigureAwait(false);
                                    
                                    return;
                                }
                            }
                            else
                            {
                                logger.LogError($"{context.TraceIdentifier}: Exception occured but could not be catched.");
                            }

                            await context.Response.WriteAsync("500 Internal Server Error").ConfigureAwait(false);
                        }));

            app.UseRouting();

            app.UseEndpoints(
                endpoints =>
                    endpoints.MapControllers());

            hostApplicationLifetime.ApplicationStarted.Register(async () =>
            {
                using (var serviceScope = app.ApplicationServices.CreateScope())
                {
                    var otaContext = serviceScope.ServiceProvider.GetRequiredService<OTAContext>();

                    otaContext.Database.SetCommandTimeout(TimeSpan.FromMinutes(10));

                    var infrastructure = otaContext.GetInfrastructure();

                    var migrator = infrastructure.GetService<IMigrator>();

                    await migrator.MigrateAsync();
                }
            });
        }
    }
}
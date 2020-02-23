using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NETCoreWebJob.Domain;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NETCoreWebJob
{
    public class Program
    {
        private static IConfiguration Configuration { get; set; }
        static async Task Main(string[] args)
        {
            var builder = new HostBuilder();
            builder.ConfigureWebJobs(b =>
            {
                b.AddAzureStorageCoreServices();
                b.AddAzureStorage();
                b.AddTimers();
                b.AddServiceBus(sbOptions =>
                {
                    sbOptions.ConnectionString = Configuration["AzureWebJobsServiceBus"];
                });
            });

            builder.ConfigureServices((context, s) => { ConfigureServices(s); s.BuildServiceProvider(); });
            builder.ConfigureLogging(logging =>
            {
                string appInsightsKey = Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"];
                if (!string.IsNullOrEmpty(appInsightsKey))
                {
                    logging.ClearProviders();
                    logging.AddConfiguration(Configuration);
                    
                    // This uses the options callback to explicitly set the instrumentation key.
                    logging.AddApplicationInsights(appInsightsKey)
                           .SetMinimumLevel(LogLevel.Information);
                    logging.AddApplicationInsightsWebJobs(o => { o.InstrumentationKey = appInsightsKey; });
                }

            });
            var tokenSource = new CancellationTokenSource();
            CancellationToken ct = tokenSource.Token;
            var host = builder.Build();
           
            using (host)
            {
                await host.RunAsync(ct);
                tokenSource.Dispose();
            }
        }
        private static void ConfigureServices(IServiceCollection services)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            Environment.SetEnvironmentVariable("MyConnVariable", Configuration["MyConnVariable"]);

            #region RegisterServiceProviders
            var dbConn = Configuration["MyConnVariable"];
            services.AddMemoryCache();
            services.AddSingleton(Configuration);
            services.AddScoped<Functions, Functions>();
            services.AddScoped<IDoWork, DoWork>();
            services.AddScoped<IMyDBContext>((o) =>
            {
                var options = new DbContextOptionsBuilder<MyDBContext>().UseSqlServer(dbConn).Options;
                return new MyDBContext(options);
            });
            #endregion

        }
    }
}

using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using SingLife.PolicySystem.Shared.Configuration;
using SingLife.PolicySystem.Shared.Logging;
using System;

namespace SingLife.ULTracker.WebAPI
{
    public class Program
    {
        private static IConfiguration Configuration => new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .Enrich.WithHttpUserName()
                .CreateLogger();

            try
            {
                Log.Logger.Information("Starting HNWAdmin Web API...");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Logger.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                var disposableLogger = Log.Logger as IDisposable;
                disposableLogger?.Dispose();
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            // We need to initialize the application at this very early stage, even before the host builder
            // is created, to make sure that the application settings are available. For example,
            // Products.Initialize() must be called before anything accesses it.
            ApplicationInitialization.Initialize(Configuration);
            ApplicationSettings.Initialize(Configuration);

            return Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureWebHostDefaults(configure =>
                {
                    configure
                        .UseStartup<Startup>()
                        .UseSerilog(Log.Logger);
                });
        }
    }
}
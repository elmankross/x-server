using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;

namespace X_Server
{
    public class Program
    {
        public static int Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            var argumentsBuilder = new CliArgumentsBuilder();
            var argumentsResult = argumentsBuilder.Parse(args);
            if (argumentsResult.HasErrors)
            {
                Log.Fatal(argumentsResult.ErrorText);
                return 1;
            }

            try
            {
                Log.Information("Starting web host.");
                CreateHostBuilder(argumentsBuilder.Object).Build().Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly.");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(CliArguments args) =>
            Host.CreateDefaultBuilder()
                .UseSerilog((context, config) =>
                {
                    config.ReadFrom.Configuration(context.Configuration);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    if (args.Config != null)
                    {
                        webBuilder.ConfigureAppConfiguration((_, x) => x.AddJsonFile(args.Config, true));
                    }
                    webBuilder.UseStartup<Startup>();
                });
    }
}

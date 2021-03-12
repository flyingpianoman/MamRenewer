using Hangfire;
using Hangfire.Common;
using Hangfire.Storage.SQLite;
using MamRenewer.Jobs;
using MamRenewer.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace MamRenewer
{
    class Program
    {
        public const string ProxiedHttpClientName = "ProxiedHttpClient";

        public static Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            
            var recurringJobManager = host.Services.GetRequiredService<Hangfire.IRecurringJobManager>();
            recurringJobManager.AddOrUpdate<SignInToMamJob>(nameof(SignInToMamJob), 
                job => job.ExecuteAsync(), Cron.Minutely());

            return host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(cb =>
                {
                    cb.AddEnvironmentVariables();
                    cb.AddJsonFile("appsettings.json");
                    cb.AddJsonFile("appsettings.localdev.json");
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHttpClient(ProxiedHttpClientName)
                        .ConfigurePrimaryHttpMessageHandler(() =>
                        {
                            return new HttpClientHandler
                            {
                                Proxy = new WebProxy(hostContext.Configuration.GetValue<string>("Proxy:Address"))
                            };
                        });

                    services.AddHangfire(c =>
                    {
                        c.UseSQLiteStorage(hostContext.Configuration.GetConnectionString("HangfireConnection"));
                    });
                    services.AddHangfireServer(o =>
                    {
                        o.WorkerCount = 1;
                    });

                    services.AddTransient<SignInToMamJob>();
                    services.AddTransient<PreviousJobInfoRepository>();
                })
                .UseConsoleLifetime();
    }
}

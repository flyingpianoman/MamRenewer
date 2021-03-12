using Hangfire;
using Hangfire.Common;
using Hangfire.Storage.SQLite;
using MamRenewer.Jobs;
using MamRenewer.Mam;
using MamRenewer.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using System;
using System.IO;
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

            //Create dir for screenshots
            var configuration = host.Services.GetRequiredService<IConfiguration>();
            Directory.CreateDirectory(configuration.GetValue<string>("Selenium:ScreenshotDir"));

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
                            var c = hostContext.Configuration;
                            var proxyEnabled = c.GetValue<bool>("Proxy:Enabled");
                            var address = c.GetValue<string>("Proxy:Address");

                            return proxyEnabled
                                ? new HttpClientHandler
                                    {
                                        Proxy = new WebProxy(address)
                                    }
                                : new HttpClientHandler();
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
                    services.AddTransient<MamBot>();

                    services.AddTransient<IWebDriver>(sp =>
                    {
                        var configuration = sp.GetRequiredService<IConfiguration>();
                        var capabilities = new OpenQA.Selenium.Firefox.FirefoxOptions().ToCapabilities();
                        return new RemoteWebDriver(configuration.GetValue<Uri>("Selenium:SeleniumHubAddress"), capabilities);
                    });
                })
                .UseConsoleLifetime();
    }
}

using Hangfire;
using Hangfire.Common;
using Hangfire.Storage.SQLite;
using MamRenewer.Jobs;
using MamRenewer.Mam;
using MamRenewer.Mam.Pages;
using MamRenewer.Services;
using MamRenewer.UINavigation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
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
            Directory.CreateDirectory(configuration.GetValue<string>("MamBot:ScreenshotDir"));

            var recurringJobManager = host.Services.GetRequiredService<Hangfire.IRecurringJobManager>();
            recurringJobManager.AddOrUpdate<RenewMamVipJob>(nameof(RenewMamVipJob),
                job => job.ExecuteAsync(), Cron.Weekly());
            recurringJobManager.AddOrUpdate<RefreshMamIPJob>(nameof(RefreshMamIPJob),
                job => job.ExecuteAsync(), Cron.Minutely());

            return host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(cb =>
                {
                    cb.AddJsonFile("appsettings.json");
                    cb.AddJsonFile("appsettings.localdev.json", optional: true);
                    cb.AddEnvironmentVariables();
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

                    services.AddTransient<RenewMamVipJob>();
                    services.AddTransient<RefreshMamIPJob>();
                    services.AddTransient<PreviousJobInfoRepository>();
                    services.AddTransient<MamBot>();
                    services.AddTransient<ScreenShotter>();
                    services.AddTransient<LoginPage>();
                    services.AddTransient<HomePage>();
                    services.AddTransient<StorePage>();

                    services.AddSingleton<IWebDriver>(sp =>
                    {
                        var configuration = sp.GetRequiredService<IConfiguration>();
                        var proxyEnabled = configuration.GetValue<bool>("Proxy:Enabled");
                        var address = configuration.GetValue<string>("Proxy:Address");

                        var proxySettings = proxyEnabled
                            ? new Proxy { HttpProxy = address, SslProxy = address, Kind = ProxyKind.Manual }
                            : null;

                        var capabilities = new FirefoxOptions()
                        {
                            Proxy = proxySettings
                        }.ToCapabilities();

                        return new RemoteWebDriver(configuration.GetValue<Uri>("MamBot:SeleniumHubAddress"), capabilities);
                    });
                })
                .UseConsoleLifetime();
    }
}

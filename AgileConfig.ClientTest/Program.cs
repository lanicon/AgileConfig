﻿using Agile.Config.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Agile.Config.ClientTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var lf = serviceProvider.GetService<ILoggerFactory>();

            var appId = "xxx";
            var seret = "app1";
            var host = "http://localhost:5000";

            try
            {
                AgileConfig.Logger = lf.CreateLogger<IConfigClient>();
                AgileConfig.AppId = appId;
                AgileConfig.Secret = seret;
                AgileConfig.ServerNodes = host;

                var client = AgileConfig.ClientInstance;
                var provider = new AgileConfigProvider(client, lf);
                provider.Load();
                Task.Run(async () =>
                {
                    while (true)
                    {
                        await Task.Delay(5000);
                        foreach (string key in client.Data.Keys)
                        {
                            var val = client[key];
                            Console.WriteLine("{0} : {1}", key, val);
                        }
                    }
                });
                for (int i = 0; i < 30; i++)
                {
                    Task.Run(provider.Load);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }



            Console.ReadLine();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(configure => configure.AddConsole());
            services.Configure<LoggerFilterOptions>(op =>
            {
                op.MinLevel = LogLevel.Trace;
            });
        }
    }
}

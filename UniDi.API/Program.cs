using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UNIONTEK.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            //var config = new ConfigurationBuilder()
            //    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            //    .AddEnvironmentVariables()
            //    .Build();

            IHost host = CreateHostBuilder(args).Build();
            

            using (var scope = host.Services.CreateScope())
            {
                var ipPolicyStore = scope.ServiceProvider.GetRequiredService<IIpPolicyStore>();
                await ipPolicyStore.SeedAsync();
            }  
            await host.RunAsync();
        }
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}

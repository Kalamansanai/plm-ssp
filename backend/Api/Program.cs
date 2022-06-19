using System.IO;
using Api.Infrastructure.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args);

            host.UseLogging();
            host.ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });

            return host;
        }
    }
}
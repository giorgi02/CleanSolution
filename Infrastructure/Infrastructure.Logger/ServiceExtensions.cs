using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Infrastructure.Logger;
public static class ServiceExtensions
{
    // დაილოგება Seq ლოგების მენეჯერში
    public static void AddLoggerLayer(this IHostBuilder host, IConfiguration configuration)
    {
        host.UseSerilog((context, config) => config
            .ReadFrom.Configuration(configuration)
            .Enrich.WithProperty("Project", "[CleanSolution]")
            .WriteTo.Seq("http://localhost:5341", period: new TimeSpan(0, 0, 10)));
    }
}
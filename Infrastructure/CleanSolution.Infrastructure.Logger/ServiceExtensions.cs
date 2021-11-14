using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Workabroad.Infrastructure.Logger;
public static class ServiceExtensions
{
    //public static void AddLoggerLayer(this IServiceCollection services, IConfiguration configuration)
    //{
    //    ServiceExtensions.LogToSeq(configuration);
    //}

    //// დაილოგება Seq ლოგების მენეჯერში
    //private static void LogToSeq(IConfiguration configuration)
    //{
    //    Log.Logger = new LoggerConfiguration()
    //            .ReadFrom.Configuration(configuration)
    //            .WriteTo.Seq("http://localhost:5341")
    //            .CreateLogger();
    //}

    public static void AddLoggerLayer(this IHostBuilder host, IConfiguration configuration)
    {
        host.UseSerilog((context, config) => config
            .ReadFrom.Configuration(configuration)
            .WriteTo.Seq("http://localhost:5341"));
    }
}
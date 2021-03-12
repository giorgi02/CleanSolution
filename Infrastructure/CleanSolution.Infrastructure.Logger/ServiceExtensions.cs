using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using System;
using System.Collections.ObjectModel;
using System.Data;

namespace Workabroad.Infrastructure.Logger
{
    public static class ServiceExtensions
    {
        public static void AddLoggerLayer(this IServiceCollection services, IConfiguration configuration)
        {
            //პროექტის გაშვებამდე ლოგის შესაქმნელად:
            /*            var appSettings = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json")
                            .Build();*/


            var logDB = configuration.GetConnectionString("DefaultConnection");


            var sinkOpts = new MSSqlServerSinkOptions
            {
                TableName = "LogEvents",
                SchemaName = "dbo",
                AutoCreateSqlTable = true,
                BatchPostingLimit = 1000,
                BatchPeriod = new TimeSpan(0, 0, 10)
            };

            var columnOpts = new ColumnOptions()
            {
                AdditionalColumns = new Collection<SqlColumn>
                {
                    new SqlColumn
                    {
                        ColumnName = "AccountType",
                        PropertyName = "AccountType",
                        DataType = SqlDbType.NVarChar,
                        DataLength = 32,
                        NonClusteredIndex = false,
                        AllowNull = true
                    },
                    new SqlColumn
                    {
                        ColumnName = "AccountId",
                        DataType = SqlDbType.UniqueIdentifier,
                        NonClusteredIndex = true,
                        AllowNull = true
                    }
                }
            };

            // we don't need XML data
            //columnOpts.Store.Remove(StandardColumn.Properties);
            // we do want JSON data
            columnOpts.Store.Add(StandardColumn.LogEvent);

            Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(configuration)
                    .WriteTo.MSSqlServer(
                        connectionString: logDB,
                        sinkOptions: sinkOpts,
                        columnOptions: columnOpts)
                    .CreateLogger();
        }
    }
}

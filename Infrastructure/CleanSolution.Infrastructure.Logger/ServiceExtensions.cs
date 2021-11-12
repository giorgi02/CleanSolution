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
            //ServiceExtensions.LogToSql(configuration);
            ServiceExtensions.LogToSeq(configuration);
        }
        // დაილოგება SqlServer მონაცემთა ბაზაში
        private static void LogToSql(IConfiguration configuration)
        {
            //// პროექტის გაშვებამდე ლოგის შესაქმნელად:
            //var appSettings = new ConfigurationBuilder()
            //    .SetBasePath(Directory.GetCurrentDirectory())
            //    .AddJsonFile("appsettings.json")
            //    .Build();

            var logDb = configuration.GetConnectionString("DefaultConnection");

            var sinkOpts = new MSSqlServerSinkOptions
            {
                TableName = "LogActions",
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
                        ColumnName = "UserId",
                        DataType = SqlDbType.UniqueIdentifier,
                        NonClusteredIndex = true,
                        AllowNull = true
                    },
                    new SqlColumn
                    {
                        ColumnName = "Method",
                        PropertyName = "RequestMethod",
                        DataType = SqlDbType.NVarChar,
                        DataLength = 16,
                        NonClusteredIndex = false,
                        AllowNull = true
                    },
                    new SqlColumn
                    {
                        ColumnName = "TraceId",
                        DataType = SqlDbType.NVarChar,
                        DataLength = 32,
                        NonClusteredIndex = false,
                        AllowNull = true
                    }
                }
            };

            // we don't need XML data
            columnOpts.Store.Remove(StandardColumn.Properties);
            // we do want JSON data
            columnOpts.Store.Add(StandardColumn.LogEvent);

            Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(configuration)
                    .WriteTo.MSSqlServer(
                        connectionString: logDb,
                        sinkOptions: sinkOpts,
                        columnOptions: columnOpts)
                    .CreateLogger();
        }
        // დაილოგება Seq ლოგების მენეჯერში
        private static void LogToSeq(IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(configuration)
                    .WriteTo.Seq("http://localhost:5341")
                    .CreateLogger();
        }
    }
}

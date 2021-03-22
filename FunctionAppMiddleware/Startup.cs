using FunctionAppMiddleware;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Azure.WebJobs;
using HttpRequestMiddleware.CLI;
using Serilog;
using Serilog.Events;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Exceptions;

[assembly: WebJobsStartup(typeof(Startup))]
namespace FunctionAppMiddleware
{
    public class Startup : IWebJobsStartup
    {

        public void Configure(IWebJobsBuilder builder)
        {

            //var loggerFactory = bu.GetService<ILoggerFactory>();
            //builder.Services.AddLogging(x =>
            //{

            //});

            //var logger = builder.Services..CreateLogger($"{RuntimeModulesNamespace}.LogHttpRequestHeaders");

            var logger = new LoggerConfiguration()
              .MinimumLevel.Information()
              .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
              .MinimumLevel.Override("System", LogEventLevel.Warning)
              .Enrich.WithExceptionDetails()
              .Enrich.FromLogContext()
    #if DEBUG
               .MinimumLevel.Verbose()
              .WriteTo.Debug()
    #endif
               .WriteTo.Console()
              .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
              .CreateLogger();


            builder.Services.AddLogging(p => p.AddSerilog(logger));
            builder.Services.LogHttpRequestHeaders(keys : new string[] { "X-Rate-Limit-Limit", "X-Rate-Limit-Remaining", "X-Rate-Limit-Reset" });
        }
        
    }
  
}

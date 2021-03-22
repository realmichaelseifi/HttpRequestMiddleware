using FunctionAppMiddleware;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Azure.WebJobs;
using HttpRequestMiddleware.CLI;
using Serilog;
using Serilog.Events;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Exceptions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: WebJobsStartup(typeof(Startup))]
namespace FunctionAppMiddleware
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
       
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
            builder.Services.AddHttpRequestHeadersLogger(
                keys : new string[] { "X-Rate-Limit-Limit", "X-Rate-Limit-Remaining", "X-Rate-Limit-Reset" });
        }
        
    }
  
}

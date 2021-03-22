using Microsoft.Extensions.DependencyInjection;
using HttpRequestMiddleware.CLI.DependencyInjection;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;

[assembly: FunctionsStartup(typeof(FunctionAppDependenyInjection.Startup))]
namespace FunctionAppDependenyInjection
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



            builder.Services.AddHttpContextAccessor();
            // MIDDLEWARE PIPELINE FACTORY
            builder.Services.AddTransient<IMiddlewarePipelineFactory, MiddlewarePipelineFactory>();
            // MIDDLEWARES TO INJECT INTO THE FACTORY 
            builder.Services.AddHttprequestHeaderLogMiddleware(options =>
            {
                options.Keys = new string[] { "X-Rate-Limit-Limit", "X-Rate-Limit-Remaining", "X-Rate-Limit-Reset" };
            });
            // builder.Services.AddTransient<OTHER CUSTOM MIDDLEWARE>();
            // ....
        }
    }
}

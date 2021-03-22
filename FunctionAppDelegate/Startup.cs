using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System;
using HttpRequestMiddleware.CLI.MessageHandler;
using Polly;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;


[assembly: FunctionsStartup(typeof(FunctionApp2.Startup))]
namespace FunctionApp2
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
            // Microsoft.Extensions.Http
            builder.Services
                .AddHttprequestHeaderLogDeleagateHandler(o =>
                {
                    o.Keys = new string[] { "X-Rate-Limit-Limit", "X-Rate-Limit-Remaining", "X-Rate-Limit-Reset" };
                })
                .AddHttpClient("Header_logger", c =>
                {
                    // c.BaseAddress = new Uri("https:/....");
                    // c.DefaultRequestHeaders.Add(name: "Accept", value: "");
                    c.DefaultRequestHeaders.Add(name: "HTTP_HEADER_LOG", "1");
                })
                // HttpMessageHandler has a default lifetime of two minutes. Increase this to say <5 minutes>
                .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                .AddTransientHttpErrorPolicy(x =>
                {
                    return x.WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(300));
                })
                // chain your handlers
                .AddHttpMessageHandler<HttprequestHeaderLogDeleagateHandler>();

        }
    }
}

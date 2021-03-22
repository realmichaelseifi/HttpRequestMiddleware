using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System;
using HttpRequestMiddleware.CLI.MessageHandler;
using Polly;

[assembly: FunctionsStartup(typeof(FunctionApp2.Startup))]
namespace FunctionApp2
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Microsoft.Extensions.Http
            builder.Services
                .AddSingleton<HttprequestHeaderLogDeleagateHandler>()
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

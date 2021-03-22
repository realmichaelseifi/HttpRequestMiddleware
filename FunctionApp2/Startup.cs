using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using HttpRequestMiddleware.CLI.DependencyInjection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

[assembly: FunctionsStartup(typeof(FunctionApp2.Startup))]
namespace FunctionApp2
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpContextAccessor();
            // middleware factory
            builder.Services.AddTransient<IMiddlewarePipelineFactory, MiddlewarePipelineFactory>();
            // middlewares to inject into factory 
            builder.Services.AddTransient<HttprequestHeaderMiddleware>();
            // builder.Services.AddTransient<Middleware2>();
            // ....
        }
    }
}

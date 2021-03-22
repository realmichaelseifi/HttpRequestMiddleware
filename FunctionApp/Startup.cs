using FunctionApp;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Azure.WebJobs;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using Microsoft.Extensions.Logging;
using HttpRequestMiddleware.CLI;
using Microsoft.Extensions.Hosting;

[assembly: WebJobsStartup(typeof(Startup))]
namespace FunctionApp
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

            builder.Services.LogHttpRequestHeaders(keys : new string[] { "X-Rate-Limit-Limit", "X-Rate-Limit-Remaining", "X-Rate-Limit-Reset" });
        }
        
    }
  
}

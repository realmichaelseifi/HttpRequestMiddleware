using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HttpRequestMiddleware.CLI.DependencyInjection
{
    public class Middleware1 : HttpMiddleware
    {
        private readonly ILogger<Middleware1> logger;

        public Middleware1(ILogger<Middleware1> logger)
        {
            this.logger = logger;
        }
        public override async Task InvokeAsync(HttpContext context)
        {
            context.Response.Headers["x-middleware-a"] = "Hello from middleware A";
            this.logger.LogInformation("Invoking Middleware1");
            if (this.Next != null)
            {
                await this.Next.InvokeAsync(context);
            }
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HttpRequestMiddleware.CLI.DependencyInjection
{
    public class HttprequestHeaderMiddleware : HttpMiddleware
    {
        private readonly ILogger<HttprequestHeaderMiddleware> logger;

        public IOptions<HttprequestHeaderLogOptions> Options { get; }

        public HttprequestHeaderMiddleware(
            ILogger<HttprequestHeaderMiddleware> logger,
             IOptions<HttprequestHeaderLogOptions> options)
        {
            this.logger = logger;
            this.Options = options;
        }
        public override async Task InvokeAsync(HttpContext context)
        {
            context.Response.Headers["x-middleware-a"] = "Hello from middleware A";
            this.logger.LogInformation("Invoking Middleware1");

            var headers = context.Request.Headers;
            foreach (var key in this.Options?.Value.Keys)
            {

                if (headers.ContainsKey(key))
                    logger.LogDebug("Header {0} : {1}", key, headers[key]);
            }

            if (this.Next != null)
            {
                await this.Next.InvokeAsync(context);
            }
        }
    }
}

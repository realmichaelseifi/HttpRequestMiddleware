using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HttpRequestMiddleware.CLI.DependencyInjection
{
    public class MiddlewarePipelineFactory : IMiddlewarePipelineFactory
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly Middleware1 middleware;
        private readonly IMiddlewarePipeline pipeline;

        public IMiddlewarePipeline Pipeline => this.pipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="MiddlewareFactory"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        /// <param name="middlewareA">Middleware A</param>
        /// <param name="middlewareB">Middleware B</param>
        public MiddlewarePipelineFactory(
            IHttpContextAccessor httpContextAccessor,
            ILogger<MiddlewarePipelineFactory> logger,
            Middleware1 middleware)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.middleware = middleware;
            this.pipeline =  this.Create(ExecuteFunction1Async);
            logger.LogInformation("logging ...");
        }

        private async Task<IActionResult> ExecuteFunction1Async(HttpContext context)
        {
            await Task.CompletedTask;

            var payload = new
            {
                message = "OK",
                functionName = "Function1"
            };

            return new OkObjectResult(payload);
        }

        /// <summary>
        /// Creates a pipeline to validate query parameters.
        /// </summary>
        /// <typeparam name="TQuery">The object type representing the query parameters.</typeparam>
        /// <param name="func">The method containing the Azure Function business logic implementation.</param>
        /// <returns>The middleware pipeline.</returns>
        public IMiddlewarePipeline Create(Func<HttpContext, Task<IActionResult>> func)
        {
            MiddlewarePipeline pipeline = new MiddlewarePipeline(this.httpContextAccessor);

            // If Function1 is called, then use MiddlewareA and B, else use MiddlewareB only
            return pipeline.UseWhen(ctx => ctx.Request.Path.StartsWithSegments("/api"),
                                    p => p.Use(middleware))

                           .Use(func);
        }
    }
}

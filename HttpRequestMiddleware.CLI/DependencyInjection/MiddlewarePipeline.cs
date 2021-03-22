using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpRequestMiddleware.CLI.DependencyInjection
{
   public  class MiddlewarePipeline : IMiddlewarePipeline
    {
        private readonly List<IHttpMiddleware> pipeline = new List<IHttpMiddleware>();
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="MiddlewarePipeline"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        public MiddlewarePipeline(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Adds middleware to the pipeline.
        /// </summary>
        /// <param name="middleware">The middleware to add.</param>
        /// <returns>The pipeline.</returns>
        public IMiddlewarePipeline Use(IHttpMiddleware middleware)
        {
            if (this.pipeline.Any())
            {
                this.pipeline.Last().Next = middleware;
            }

            this.pipeline.Add(middleware);

            return this;
        }

        /// <summary>
        /// Executes the pipeline.
        /// </summary>
        /// <returns>The value to returned from the Azure function.</returns>
        public async Task<IActionResult> RunAsync()
        {
            var context = this.httpContextAccessor.HttpContext;

            if (this.pipeline.Any())
            {
                await this.pipeline.First().InvokeAsync(context);
            }
            else
            {
                throw new MiddlewarePipelineException($"No middleware configured");
            }

            return new HttpResponseResult(context);
        }

        /// <summary>
        /// Creates a new pipeline with the same configuration as the current instance.
        /// </summary>
        /// <returns>The new pipeline.</returns>
        public IMiddlewarePipeline New()
        {
            return new MiddlewarePipeline(this.httpContextAccessor);
        }
    }
}

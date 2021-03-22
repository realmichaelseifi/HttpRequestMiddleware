using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HttpRequestMiddleware.CLI.DependencyInjection
{
     public static class MiddlewarePipelineExtensions
    {
        /// <summary>
        /// Conditionally creates a branch in the request pipeline that is rejoined to the main pipeline.
        /// </summary>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="condition">The function which is invoked to determine if the branch should be taken.</param>
        /// <param name="configure">Configures the branch.</param>
        /// <returns>The pipeline instance.</returns>
        public static IMiddlewarePipeline UseWhen(
            this IMiddlewarePipeline pipeline,
            Func<HttpContext, bool> condition,
            Action<IMiddlewarePipeline> configure)
        {
            var middleware = new ConditionalMiddleware(pipeline, condition, configure, true);
            return pipeline.Use(middleware);
        }

        /// <summary>
        /// Adds an Azure Function middleware to the pipeline.
        /// </summary>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="func">The function to add.</param>
        /// <returns>The pipeline instance.</returns>
        public static IMiddlewarePipeline Use(this IMiddlewarePipeline pipeline, Func<HttpContext, Task<IActionResult>> func)
        {
            return pipeline.Use(new FunctionMiddleware(func));
        }
    }
}

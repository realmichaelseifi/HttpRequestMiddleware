﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HttpRequestMiddleware.CLI.DependencyInjection
{
    public interface IMiddlewarePipeline
    {
        /// <summary>
        /// Adds middleware to the pipeline.
        /// </summary>
        /// <param name="middleware">The middleware to add.</param>
        /// <returns>The pipeline.</returns>
        IMiddlewarePipeline Use(IHttpMiddleware middleware);

        /// <summary>
        /// Executes the pipeline.
        /// </summary>
        /// <returns>The value to returned from the Azure function.</returns>
        Task<IActionResult> RunAsync();

        /// <summary>
        /// Creates a new pipeline with the same configuration as the current instance.
        /// </summary>
        /// <returns>The new pipeline.</returns>
        IMiddlewarePipeline New();
    }
}

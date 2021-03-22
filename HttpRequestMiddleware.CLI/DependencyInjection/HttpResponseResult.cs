using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HttpRequestMiddleware.CLI.DependencyInjection
{
    public class HttpResponseResult : IActionResult
    {
        private readonly HttpContext context;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpResponseResult"/> class.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        public HttpResponseResult(HttpContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Gets the HTTP context.
        /// </summary>
        public HttpContext Context => this.context;

        /// <summary>
        /// Executes the result operation of the action method asynchronously.
        /// </summary>
        /// <param name="context">
        /// The context in which the result is executed. The context information
        /// includes information about the action that was executed and request
        /// information.
        /// </param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ExecuteResultAsync(ActionContext context)
        {
            await Task.CompletedTask;
            context.HttpContext = this.context;
        }
    }
}

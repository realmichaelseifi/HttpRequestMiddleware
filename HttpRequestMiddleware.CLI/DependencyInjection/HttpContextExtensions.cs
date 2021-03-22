using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HttpRequestMiddleware.CLI.DependencyInjection
{
   public static  class HttpContextExtensions
    {
        public static async Task ProcessActionResultAsync(this HttpContext context, IActionResult result)
        {
            await result.ExecuteResultAsync(new ActionContext(context, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()));
        }
    }
}

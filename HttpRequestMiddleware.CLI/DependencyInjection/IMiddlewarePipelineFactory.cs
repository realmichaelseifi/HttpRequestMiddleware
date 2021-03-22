using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace HttpRequestMiddleware.CLI.DependencyInjection
{
    public interface IMiddlewarePipelineFactory
    {
        IMiddlewarePipeline Create(Func<HttpContext, Task<IActionResult>> func);
        IMiddlewarePipeline Pipeline { get; }

    }

    
}
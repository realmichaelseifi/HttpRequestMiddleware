using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace HttpRequestMiddleware.CLI.DependencyInjection
{
    public interface IHttpMiddleware
    {
        IHttpMiddleware Next { get; set; }

        Task InvokeAsync(HttpContext context);
    }
}
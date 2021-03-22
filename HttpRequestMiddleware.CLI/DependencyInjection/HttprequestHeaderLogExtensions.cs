using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace HttpRequestMiddleware.CLI.DependencyInjection
{
    public static class HttprequestHeaderLogExtensions
    {
        public static IServiceCollection AddHttprequestHeaderLogMiddleware(this IServiceCollection service
            , Action<HttprequestHeaderLogOptions> options)
        {
            service.Configure(options);
            service.AddTransient<HttprequestHeaderMiddleware>();
            return service;
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace HttpRequestMiddleware.CLI.MessageHandler
{
    public static class HttprequestHeaderLogDeleagateHandlerExtensions
    {
        public static IServiceCollection AddHttprequestHeaderLogDeleagateHandler(this IServiceCollection service
            , Action<HttprequestHeaderLogOptions> options)
        {
            service.Configure(options);
            service.AddSingleton<HttprequestHeaderLogDeleagateHandler>();
            return service;
        }
    }
}

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HttpRequestMiddleware.CLI.MessageHandler
{
    public class HttprequestHeaderLogDeleagateHandler : DelegatingHandler
    {
        private readonly ILogger<HttprequestHeaderLogDeleagateHandler> logger;

        public HttprequestHeaderLogDeleagateHandler( ILogger<HttprequestHeaderLogDeleagateHandler> logger)
        {
            this.logger = logger;
        }

        

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);
            var headers = response.Headers; //.GetValues("test");
            return response;
        }

    }
}

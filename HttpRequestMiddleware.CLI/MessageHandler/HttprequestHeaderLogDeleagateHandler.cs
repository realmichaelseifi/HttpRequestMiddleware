using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace HttpRequestMiddleware.CLI.MessageHandler
{
    public class HttprequestHeaderLogDeleagateHandler : DelegatingHandler
    {
        private readonly ILogger<HttprequestHeaderLogDeleagateHandler> logger;

        public IOptions<HttprequestHeaderLogOptions> Options { get; }
        public IHttpContextAccessor HttpAccessor { get; }

        public HttprequestHeaderLogDeleagateHandler(
            ILogger<HttprequestHeaderLogDeleagateHandler> logger,
            IHttpContextAccessor httpContextAccessor,
            IOptions<HttprequestHeaderLogOptions> options)
        {
            this.logger = logger;
            this.Options = options;
            this.HttpAccessor = httpContextAccessor;
            
            // WE CAN LOG HERE, BUT THIS WILL GET CALLED ONCE IN THE CONSTRUCTOR OF THE FUNCTION
            // LogHeaders();
        }

        

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // THIS WILL GET CALLED WITH EVERY FUNCTION CALL
            LogHeaders();
            return await base.SendAsync(request, cancellationToken); ;
        }

        private void LogHeaders()
        {
            var headers = this.HttpAccessor?.HttpContext.Request.Headers; //.GetValues("test");


            foreach (var key in this.Options?.Value.Keys)
            {

                if (headers.ContainsKey(key))
                    logger.LogDebug("Header {0} : {1}", key, headers[key]);
            }
        }

    }
}

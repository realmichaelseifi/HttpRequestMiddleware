using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace FunctionAppDelegate
{
    public class HttprequestHeaderClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            return new HttpClient();
        }
    }
}

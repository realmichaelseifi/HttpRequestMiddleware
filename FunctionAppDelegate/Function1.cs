using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using HttpRequestMiddleware.CLI.MessageHandler;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FunctionAppDelegate
{
    public  class Function1
    {
        public Function1(IHttpClientFactory httpFactory)
        {
            this.HttpFactory = httpFactory;
            this.HttpClient =  this.HttpFactory.CreateClient("Header_logger");
        }

         public IHttpClientFactory HttpFactory { get; }
        public HttpClient HttpClient { get; }

        [FunctionName("Function1")]
        public  async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");


            var kdk = await this.HttpClient.GetAsync("http://localhost:7071/api/TestFunction?name=michael");
            
            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP Post triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }


        [FunctionName("TestFunction")]
        public async Task<IActionResult> TestGet(
             [HttpTrigger(AuthorizationLevel.Anonymous, "get",  Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");


            var kdk = await this.HttpClient.GetAsync("https://google.com");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }
    }
}


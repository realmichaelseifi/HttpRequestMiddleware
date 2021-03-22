using System.IO;
using System.Threading.Tasks;
using HttpRequestMiddleware.CLI.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FunctionAppDependenyInjection
{


    public  class Function1
    {
        private readonly IMiddlewarePipelineFactory pipelineFactory;
        public Function1(IMiddlewarePipelineFactory pipelineFactory)
        {
            this.pipelineFactory = pipelineFactory;
        }


        [FunctionName("Function1")]
        public  async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");


            // CREATE PIPELINE PER FUNCTION CALL
            // var pipeline =  this.pipelineFactory.Create(ExecuteFunction1Async);

            // CALL PIPELINE.
            // THIS WILL CALL ALL INJECTED MIDDLEWARES THAT HAVE A MATCHING STARTING REQUEST PATH
            _ = await this.pipelineFactory.Pipeline.RunAsync();
            // _ = await pipeline.RunAsync();

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }

        private async Task<IActionResult> ExecuteFunction1Async(HttpContext context)
        {
            await Task.CompletedTask;

            // THERE IS A NEED TO RETURN A PAYLOAD
            var payload = new
            {
                message = "OK",
                functionName = "Function1"
            };

            return new EmptyResult(); // OkObjectResult(payload);
        }
    }
}


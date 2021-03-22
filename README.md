# HttpRequestMiddleware

Thid project contains three ways to log request headers

# Middleware
We build a custom dynamic Middleware

# Dependency Injection
Build a Middleware Pipeline Factory
Inject the factory into the constructor of the function

Create a pipline 

      public IMiddlewarePipeline Create(Func<HttpContext, Task<IActionResult>> func)
        {
            MiddlewarePipeline pipeline = new MiddlewarePipeline(this.httpContextAccessor);

            // IF FUNCTION1 IS CALLED, THEN USE MIDDLEWAREA AND B, ELSE USE MIDDLEWAREB ONLY
            return pipeline.UseWhen(ctx => ctx.Request.Path.StartsWithSegments("/api"),
                                    p => p.Use(middleware))
                           .Use(func);
        }
        
either inside the constructor 
       
          public MiddlewarePipelineFactory(
            IHttpContextAccessor httpContextAccessor,
            ILogger<MiddlewarePipelineFactory> logger,
            HttprequestHeaderMiddleware middleware)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.middleware = middleware;

            // CREATE PIPELINE ONCE PER FUNCTION CLASS
            this.pipeline =  this.Create(ExecuteFunction1Async);

            logger.LogInformation("logging MiddlewarePipelineFactory ....");
        }
        

or at every function call

        public  async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");


            // CREATE PIPELINE PER FUNCTION CALL
            var pipeline =  this.pipelineFactory.Create(ExecuteFunction1Async);


Run the injected created pipeline inside the function

     _ = await this.pipelineFactory.Pipeline.RunAsync();

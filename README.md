# HttpRequestMiddleware

This project contains three ways to log request headers

# Middleware
Build a custom dynamic Middleware that would take a name and an Action delegate

        /// <summary>
        /// <para>CREATE A MIDDLEWARE DYNAMICALLY</para>
        /// <para>ADD THE MIDDLEWARE TO THE STACK</para>
        /// <para>ADD THE MIDDLEWARE TO SERVICE COLLECTION</para>
        /// </summary>
        private static IServiceCollection AddHttpMiddleware(
            this IServiceCollection services,
            string middlewareName,
            HttpMiddlewareDelegate @delegate)
        { 

            // IF THIS MIDDLEWARE HAS ALREADY BEEN REGISTERED, THROW AN EXCEPTION
            // THIS WILL BE CAUGHT IN DEVLOPEMNT DURING TESTING/DEBUGGING
            if (TypeExists(middlewareName))
                throw new ArgumentException($"The middleware name '{middlewareName}' already exists in the dynamic module");

            #region Build the middleware
            // DEFINE A PUBLIC CLASS NAMED OUR CUSTOM NAME IN THE ASSEMBLY.
            var typeBuilder = ModuleBuilder.DefineType(
                GetMiddlewareFullName(middlewareName),
                TypeAttributes.Public |
                TypeAttributes.Class |
                TypeAttributes.AutoClass |
                TypeAttributes.AnsiClass |
                TypeAttributes.BeforeFieldInit |
                TypeAttributes.AutoLayout,
                null
            );

            typeBuilder.AddInterfaceImplementation(IJobHostHttpMiddlewareType);
            // typeBuilder.AddInterfaceImplementation(typeof(ICustomJobHostHttpMiddleware));

            // CREATE A METHOD BUILDER
            // INJECT HTTPCONTEXT
            // ADD A ACTION
            var methodName = "Invoke";
            var @parameters = new Type[2] { typeof(HttpContext), typeof(RequestDelegate) };
            var methodBuilder =
               typeBuilder.DefineMethod(
                   methodName,
                   MethodAttributes.Public | MethodAttributes.Virtual,
                   typeof(Task),
                   @parameters
            );

            var fieldName = $"{middlewareName.Trim()}_field";
            // DEFINE A PRIVATE STRING FIELD NAMED <our custom field name> IN THE TYPE.
            var fieldBuilder = typeBuilder.DefineField(
                fieldName,
                typeof(HttpMiddlewareDelegate),
                FieldAttributes.Private
            );

           
            var methodeIl = methodBuilder.GetILGenerator();

            // PUSH ONTO THE STACK ...
            methodeIl.Emit(OpCodes.Ldarg_0);
            // LOAD THE FIELD
            methodeIl.Emit(OpCodes.Ldfld, fieldBuilder);

            // PUSH EACH ARGUMENT ONTO THE STACK. THIS WILL FORWARD THE ARGUMENTS TO THE DELEGATE.
            for (int i = 0; i < @parameters.Length; i++)
            {
                methodeIl.Emit(OpCodes.Ldarg, i + 1);
            }

            // Call the delegate and return
            methodeIl.Emit(OpCodes.Callvirt, typeof(HttpMiddlewareDelegate).GetMethod(methodName));
            methodeIl.Emit(OpCodes.Ret);

            #endregion

            var middlewareType = typeBuilder.CreateType();
            var instance = Activator.CreateInstance(middlewareType);
            middlewareType
                .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(instance, @delegate);

            // REGISTER MIDDLEWARE TYPE AS A SINGLETON
            services.Add(
                ServiceDescriptor.Singleton(IJobHostHttpMiddlewareType, instance)
            );

            return services;
        }
        #endregion
    }

Create a Middleware and inject the an Action

        services.AddHttpMiddleware("LogHttpRequestHeaders",  async (context,  next) =>
            {
                var headers = context.Request.Headers;
                var loggerFactory = context.RequestServices.GetService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger($"{RuntimeModulesNamespace}.LogHttpRequestHeaders");

                foreach (var key in keys)
                {
                    if (headers != null && headers.ContainsKey(key))
                    {
                        var keyValue = headers[key];
                        
                        logger?.LogInformation(string.IsNullOrEmpty(message) ? $"{key}:{keyValue}." : message);
                    }
                }
               
                await next(context);
            });
            
Add the Middleware to services

      builder.Services.AddHttpRequestHeadersLogger(
                keys : new string[] { "X-Rate-Limit-Limit", "X-Rate-Limit-Remaining", "X-Rate-Limit-Reset" });


# Dependency Injection
Build a Middleware Pipeline Factory,
Inject the factory into the constructor of the function

Create a pipline 

        /// <summary>
        /// Creates a pipeline to validate query parameters.
        /// </summary>
        /// <typeparam name="TQuery">The object type representing the query parameters.</typeparam>
        /// <param name="func">The method containing the Azure Function business logic implementation.</param>
        /// <returns>The middleware pipeline.</returns>
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

Create a middleware

        public class HttprequestHeaderMiddleware : HttpMiddleware
        {
        private readonly ILogger<HttprequestHeaderMiddleware> logger;

        public IOptions<HttprequestHeaderLogOptions> Options { get; }

        public HttprequestHeaderMiddleware(
            ILogger<HttprequestHeaderMiddleware> logger,
             IOptions<HttprequestHeaderLogOptions> options)
        {
            this.logger = logger;
            this.Options = options;
        }
        public override async Task InvokeAsync(HttpContext context)
        {
            context.Response.Headers["x-middleware-a"] = "Hello from middleware A";
            this.logger.LogInformation("Invoking Middleware1");

            var headers = context.Request.Headers;
            foreach (var key in this.Options?.Value.Keys)
            {

                if (headers.ContainsKey(key))
                    logger.LogDebug("Header {0} : {1}", key, headers[key]);
            }

            if (this.Next != null)
            {
                await this.Next.InvokeAsync(context);
            }
        }
    }
  
Add the pipeline and the middleware in the stratup


            builder.Services.AddHttpContextAccessor();
            // MIDDLEWARE PIPELINE FACTORY
            builder.Services.AddTransient<IMiddlewarePipelineFactory, MiddlewarePipelineFactory>();
            // MIDDLEWARES TO INJECT INTO THE FACTORY 
            builder.Services.AddHttprequestHeaderLogMiddleware(options =>
            {
                options.Keys = new string[] { "X-Rate-Limit-Limit", "X-Rate-Limit-Remaining", "X-Rate-Limit-Reset" };
            });
            // builder.Services.AddTransient<OTHER CUSTOM MIDDLEWARE>();
            // ....
            
Run the injected created pipeline inside the function

     _ = await this.pipelineFactory.Pipeline.RunAsync();

# DelegatingHandler

Create a DelegatingHandler

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
    
Create the delegatinghandler extension and its options to the services

        ...
        service.Configure(options);
            service.AddSingleton<HttprequestHeaderLogDeleagateHandler>();
            return service;     

Add it in the startup

        ...
         builder.Services.AddHttpContextAccessor();
            // Microsoft.Extensions.Http
            builder.Services
                .AddHttprequestHeaderLogDeleagateHandler(o =>
                {
                    o.Keys = new string[] { "X-Rate-Limit-Limit", "X-Rate-Limit-Remaining", "X-Rate-Limit-Reset" };
                })
                .AddHttpClient("Header_logger", c =>
                {
                    // c.BaseAddress = new Uri("https:/....");
                    // c.DefaultRequestHeaders.Add(name: "Accept", value: "");
                    c.DefaultRequestHeaders.Add(name: "HTTP_HEADER_LOG", "1");
                })
                // HttpMessageHandler has a default lifetime of two minutes. Increase this to say <5 minutes>
                .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                .AddTransientHttpErrorPolicy(x =>
                {
                    return x.WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(300));
                })
                // chain your handlers
                .AddHttpMessageHandler<HttprequestHeaderLogDeleagateHandler>();
                
Inject the IHttpClientFactory in the Function's constructor,
and create a HttpClient - this could be a token handler client
              
              public Function1(IHttpClientFactory httpFactory)
                {
                    this.HttpFactory = httpFactory;
                    this.HttpClient =  this.HttpFactory.CreateClient("Header_logger");
                }
             
Call an Api
        // THIS COULD BE THE CALL TO GET A TOKEN OR VALIDATE A TOKEN
         _ = await this.HttpClient.GetAsync("http://localhost:7071/api/gettoken?name=michael");
            

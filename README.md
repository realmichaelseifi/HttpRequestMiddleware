# HttpRequestMiddleware

This project contains three ways to log request headers

# Middleware
We build a custom dynamic Middleware

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



# Dependency Injection
Build a Middleware Pipeline Factory
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


Run the injected created pipeline inside the function

     _ = await this.pipelineFactory.Pipeline.RunAsync();

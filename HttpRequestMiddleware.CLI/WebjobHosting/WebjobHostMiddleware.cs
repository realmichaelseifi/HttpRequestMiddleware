using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace HttpRequestMiddleware.CLI
{
    public delegate Task HttpMiddlewareDelegate(HttpContext context,  RequestDelegate next);
    public static class HttpMiddlewareExtensions
    {
        private const string RuntimeModulesNamespace = "Middleware.HttpRequest.CLI";
        #region JobHostHttpMiddleware

        private static Type _jobHostHttpMiddlewareType = null;
        private static Type IJobHostHttpMiddlewareType
        {
            get
            {
                if (_jobHostHttpMiddlewareType == null)
                {
                    var @interface = "Microsoft.Azure.WebJobs.Script.Middleware.IJobHostHttpMiddleware";
                    var currentDomains = AppDomain.CurrentDomain;
                    _jobHostHttpMiddlewareType = currentDomains
                        .GetAssemblies()
                        .SelectMany(t => t.GetTypes())
                        .FirstOrDefault(t => t.IsInterface && t.FullName == @interface);
                }

                return _jobHostHttpMiddlewareType;
            }
        }

        #endregion
        public static IServiceCollection LogHttpRequestHeaders(this IServiceCollection services, string message = "", params string[] keys )
        {

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


            return services;
        }

        #region Private Methods

        private static ModuleBuilder _moduleBuilder = null;

        // CREATE A DYNAMIC MODULE IN DYNAMIC ASSEMBLY.
        private static ModuleBuilder ModuleBuilder
        {
            get
            {
                if (_moduleBuilder == null)
                {
                    var assemblyName = new AssemblyName(RuntimeModulesNamespace);
                    var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
                    _moduleBuilder = assemblyBuilder.DefineDynamicModule(RuntimeModulesNamespace);
                }

                return _moduleBuilder;
            }
        }


        private static string GetMiddlewareFullName(string middlewareName) => $"{RuntimeModulesNamespace}.{middlewareName}";

        private static bool TypeExists(string middlewareName)
        {
            var type = ModuleBuilder.GetType(GetMiddlewareFullName(middlewareName), ignoreCase: true);
            return (type != null);
        }




        /// <summary>
        /// <para>CREATE A MIDDLEWARE DYNAMICALLY</para>
        /// <para>ADD THE MIDDLEWARE TO THE STACK</para>
        /// <para>ADD THE MIDDLEWARE TO SERVICE COLLECTION</para>
        /// </summary>
        /// <remarks>
        /// <para>https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.fieldbuilder?view=netcore-3.1</para>
        /// <para>https://github.com/Azure/azure-functions-host/blob/1927f5fd736931d2a6407b84d35927f2404eb8dc/src/WebJobs.Script.WebHost/Middleware/IJobHostHttpMiddleware.cs#L12</para>
        /// </remarks>
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

}

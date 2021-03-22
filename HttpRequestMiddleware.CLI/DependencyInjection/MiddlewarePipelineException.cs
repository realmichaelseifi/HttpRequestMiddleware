using System;
using System.Collections.Generic;
using System.Text;

namespace HttpRequestMiddleware.CLI.DependencyInjection
{
    [Serializable]
    public class MiddlewarePipelineException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MiddlewarePipelineException"/> class.
        /// </summary>
        public MiddlewarePipelineException()
            : base("Middleware pipeline must be configured with at least one middleware and the final middleware must return a response")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MiddlewarePipelineException"/> class.
        /// </summary>
        /// <param name="message">The message describing the error.</param>
        public MiddlewarePipelineException(string message)
            : base(message)
        {
        }
    }
}

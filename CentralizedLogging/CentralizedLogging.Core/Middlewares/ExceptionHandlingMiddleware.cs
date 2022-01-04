using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CentralizedLogging.Core
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        public ExceptionHandlingMiddleware(
            RequestDelegate next
        )
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next.Invoke(context);
                if (context.Response.StatusCode == 500)
                    await context.Response.WriteAsync("Unexpected error");
            }
            catch (Exception)
            {
                context.Response.ContentType = "application/problem+json";
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Unexpected error");
            }
        }
    }
}

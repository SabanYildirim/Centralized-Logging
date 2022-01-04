using Microsoft.AspNetCore.Http;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CentralizedLogging.Core
{
    public class RequestPerformanceMiddleware
    {
        private readonly RequestDelegate next;

        public RequestPerformanceMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            DateTime start = DateTime.Now;
            await next(context);
            DateTime end = DateTime.Now;

            if ((end - start).TotalMilliseconds > 1000)
            {
                var log = new BaseLogModel
                {
                    RequestTime = start,
                    ResponseTime = end,
                    Duration = (end - start).TotalMilliseconds,
                    Message = "Long Running Request!.."
                };
                Log.Error("{@log}", log);
            }
        }
    }
}

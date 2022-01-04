using CentralizedLogging.Core;
using DotNetCore.CAP;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CentralizedLogging.InvoiceApi.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly ILogger<InvoiceController> _logger;

        public InvoiceController(      
           ILogger<InvoiceController> logger)
        {
            _logger = logger;
        }

        [NonAction]
        [CapSubscribe("invoce.notify")]
        public async Task Notify(long orderId, [FromCap] CapHeader header)
        {
            _logger.CustomLog(header, orderId, "successfuly notification to action");   
        }
    }
}

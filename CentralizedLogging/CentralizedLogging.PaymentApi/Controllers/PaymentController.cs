using CentralizedLogging.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace CentralizedLogging.PaymentApi.Controllers
{
    [ApiController]
    [Route("api/payment")]
    public class PaymentController : Controller
    {
        private readonly IRabbitMqService _rabbitMqService;
        private readonly ILogger<PaymentController> _logger;
        public PaymentController(
            IRabbitMqService rabbitMqService,
            ILogger<PaymentController> logger)
        {
            _rabbitMqService = rabbitMqService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] CreateInvoiceRequestModel model)
        {
            //create invoice
            //..
            if (model.OrderId % 2 == 0)
            {
                _logger.LogError("Invoice not created becasuse is dublicated");
                return BadRequest(0);
            }
            else
            {
                _logger.LogInformation("Invoice successfuly create");
            }

            await _rabbitMqService.PublishAsync("invoce.notify", model.OrderId);
            return Created("", 1);
        }

        public class CreateInvoiceRequestModel
        {
            public long OrderId { get; set; }
        }
    }
}

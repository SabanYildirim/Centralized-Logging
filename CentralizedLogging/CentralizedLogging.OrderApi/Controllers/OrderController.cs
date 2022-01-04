using CentralizedLogging.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CentralizedLogging.OrderApi.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IMainHttpService _mainHttpService;
        private readonly ILogger<OrderController> _logger;
        public OrderController(
            IMainHttpService mainHttpService,
            ILogger<OrderController> logger)
        {
            _mainHttpService = mainHttpService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(long id)
        {
            return Ok(new { Id = id, OrderNumber = "Order123456", CargoId = 21, ProductId = 2, MarketPlaceId = 14, OrderQuantity = 5 });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderRequestModel model)
        {
            //validation order
            //..
            _logger.LogInformation("successfuly validation order");

            _logger.LogInformation("Order successfuly created");
            return Created("", model.OrderNumber);
        }

        [HttpPost("packing")]
        public async Task<IActionResult> PackingOrder([FromBody] PackingRequestModel model)
        {
            //others process
            //..
            _logger.LogInformation("successfuly others process order");

            //create invoice
            //..
            var invoiceResponse = await _mainHttpService.HttpRequest<long>(HttpServiceEnum.PAYMENT, $"api/payment", HttpMethod.Post, new { OrderId = model.OrderId });
            if (!invoiceResponse.IsSuccess())
            {
                _logger.LogError("Packing operation failed because invoice not create");
                return BadRequest("Packing operation failed because invoice not create");
            }

            _logger.LogInformation("successfuly packing operation");
            return Ok(model.OrderId);
        }

        public class CreateOrderRequestModel
        {
            public long UserId { get; set; }
            public long MarketPlaceId { get; set; }
            public long ProductId { get; set; }
            public long CargoId { get; set; }
            public string OrderNumber { get; set; }
            public decimal Quantity { get; set; }
            public string Address { get; set; }
        }

        public class PackingRequestModel
        {
            public long OrderId { get; set; }
            public long CargoId { get; set; }
            public long EmployeeId { get; set; }
        }
    }
}

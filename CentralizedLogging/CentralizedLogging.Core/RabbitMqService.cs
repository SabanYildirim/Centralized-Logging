using DotNetCore.CAP;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralizedLogging.Core
{
    public class RabbitMqService : IRabbitMqService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICapPublisher _capPublisher;
        public RabbitMqService(
            IHttpContextAccessor httpContextAccessor,
            ICapPublisher capPublisher
          )
        {
            _httpContextAccessor = httpContextAccessor;
            _capPublisher = capPublisher;
        }

        public async Task PublishAsync<T>(string name, T data)
        {
            try
            {
                var headers = new Dictionary<string, string>();
                headers.Add(HeaderKeyConstants.CorrelationId, _httpContextAccessor?.HttpContext?.Request.Headers[HeaderKeyConstants.CorrelationId].FirstOrDefault());
                headers.Add(HeaderKeyConstants.Version, _httpContextAccessor?.HttpContext?.Request.Headers[HeaderKeyConstants.Version].FirstOrDefault());
                headers.Add(HeaderKeyConstants.RequestSequence, _httpContextAccessor?.HttpContext?.Request.Headers[HeaderKeyConstants.RequestSequence].FirstOrDefault());
                headers.Add(HeaderKeyConstants.QueryString, _httpContextAccessor?.HttpContext?.Request.QueryString.Value);
                headers.Add(HeaderKeyConstants.RemoteIpAddress, _httpContextAccessor?.HttpContext?.Connection.RemoteIpAddress.ToString());
                headers.Add(HeaderKeyConstants.Url, _httpContextAccessor?.HttpContext?.Request.GetDisplayUrl());
                await _capPublisher.PublishAsync(name, data, headers);
            }

            catch (Exception ex)
            {

                throw;
            }

        }
    }

    public interface IRabbitMqService
    {
        Task PublishAsync<T>(string name, T data);
    }

    public class RabbitMqConfigModel
    {
        public string RabbitMqHostname { get; set; }
        public string RabbitMqUsername { get; set; }
        public string RabbitMqPassword { get; set; }
    }
}

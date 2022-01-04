using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CentralizedLogging.Core
{
    public class MainHttpService : IMainHttpService
    {
        private readonly HttpClient _client;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly List<string> _headerKeyList;

        public MainHttpService(
            HttpClient client,
            IHttpContextAccessor httpContextAccessor)
        {
            _client = client;
            _httpContextAccessor = httpContextAccessor;
            _headerKeyList = new List<string>
            {
                HeaderKeyConstants.CorrelationId,
                HeaderKeyConstants.Version,
                HeaderKeyConstants.RequestSequence
            };
        }

        public async Task<BaseResponseModel<T>> HttpRequest<T>(HttpServiceEnum httpServiceEnum, string url, HttpMethod httpMethod, object payload = null, CancellationToken ct = default)
        {
            try
            {
                _client.BaseAddress = GetUri(httpServiceEnum);
                using (var request = new HttpRequestMessage(httpMethod, url))
                {
                    foreach (var headerKey in _headerKeyList)
                    {
                        if (!request.Headers.Any(x => x.Key == headerKey) &&
                            !string.IsNullOrEmpty(_httpContextAccessor?.HttpContext?.Request.Headers[headerKey]))
                        {
                            request.Headers.Add(headerKey, _httpContextAccessor?.HttpContext?.Request.Headers[headerKey].FirstOrDefault());
                        }
                    }

                    if (payload != null)
                    {
                        var contentJson = JsonConvert.SerializeObject(payload, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, NullValueHandling = NullValueHandling.Ignore });
                        if (!string.IsNullOrEmpty(contentJson))
                        {
                            var content = new StringContent(contentJson, Encoding.UTF8, "application/json");
                            request.Content = content;
                        }
                    }

                    var responseModel = await _client.SendAsync(request, ct);
                    return new BaseResponseModel<T>
                    {
                        StatusCode = responseModel.StatusCode,
                        Data = JsonConvert.DeserializeObject<T>(await responseModel.Content.ReadAsStringAsync())
                    };
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private Uri GetUri(HttpServiceEnum httpServiceEnum)
        {
            var baseUrl = "";
            switch (httpServiceEnum)
            {
                case HttpServiceEnum.ORDER:
                    baseUrl = "http://localhost:5101/";
                    break;
                case HttpServiceEnum.INVOICE:
                    baseUrl = "http://localhost:5103/";
                    break;
                case HttpServiceEnum.PAYMENT:
                    baseUrl = "http://localhost:50409";
                    break;
                default:
                    break;
            }
            return new Uri(baseUrl);
        }
    }


    public enum HttpServiceEnum
    {
        UNKNOWN = 0,
        ORDER = 1,
        INVOICE = 2,
        PAYMENT = 3
    }
    public interface IMainHttpService
    {
        Task<BaseResponseModel<T>> HttpRequest<T>(
            HttpServiceEnum httpServiceEnum,
            string url,
            HttpMethod httpMethod,
            object payload = null,
            CancellationToken ct = default);
    }

    public class BaseResponseModel<T>
    {
        public HttpStatusCode StatusCode { get; set; }
        public T Data { get; set; }
        public bool IsSuccess()
        {
            return StatusCode == HttpStatusCode.OK || StatusCode == HttpStatusCode.Created;
        }
    }

    public class HeaderKeyConstants
    {
        public const string CorrelationId = "X-Correlation-Id";
        public const string RequestSequence = "X-Request-Sequence";
        public const string Version = "X-Version";
        public const string PreviewRequestId = "X-Preview-Request-Id";
        public const string QueryString = "X-Query-String";
        public const string RemoteIpAddress = "X-Remote-Ip-Address";
        public const string Url = "X-Url";
    }

}

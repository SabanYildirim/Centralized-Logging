using Elasticsearch.Net;
using Microsoft.Extensions.Options;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CentralizedLogging.Core
{
    public class ElasticContext : IElasticContext
    {
        private readonly ElasticClient _elasticClient;
        private readonly int _pingTimeMilliSeconds;
        private const string LogAlias = "_log";
        private readonly JsonSerializerSettings _jsonSettings;

        public ElasticContext(IOptions<ElasticSearchConfigModel> configuration)
        {
            _jsonSettings = new JsonSerializerSettings
            {
                ContractResolver = new LowercaseContractResolver()
            };

            _pingTimeMilliSeconds = configuration.Value.PingTimeMilliSeconds;
            var connectionSettings = new ConnectionSettings(new Uri(configuration.Value.ConnectionString))
                                        .RequestTimeout(TimeSpan.FromMilliseconds(_pingTimeMilliSeconds))
                                        .DisableDirectStreaming()
                                        .DefaultMappingFor<BaseLogModel>(m => m.IdProperty(p => p.RequestID)
            );

            _elasticClient = new ElasticClient(connectionSettings);
        }

        public IndexResponseModel IndexCustom<T>(string indexName, T document) where T : class
        {
            indexName = GetIndexName(indexName);
            var isExists = _elasticClient.Indices.Exists(indexName);
            if (!isExists.Exists)
            {
                var createIndexResponseModel = CreateIndexCustom<T>(indexName).Result;
                if (!createIndexResponseModel.IsValid)
                    return createIndexResponseModel;
            }
            var response = _elasticClient.LowLevel.Index<IndexResponseModel>(indexName, PostData.String(JsonConvert.SerializeObject(document, Formatting.Indented, _jsonSettings)));
            return new IndexResponseModel { IsValid = response.IsValid };
        }
        public async Task<IndexResponseModel> IndexCustomAsync<T>(string indexName, T document, CancellationToken ct = default) where T : class
        {
            indexName = GetIndexName(indexName);
            var isExists = await _elasticClient.Indices.ExistsAsync(indexName, ct: ct);
            if (!isExists.Exists)
            {
                var createIndexResponseModel = await CreateIndexCustom<T>(indexName, ct);
                if (!createIndexResponseModel.IsValid)
                    return createIndexResponseModel;
            }
            var response = await _elasticClient.LowLevel.IndexAsync<IndexResponseModel>(indexName, PostData.String(JsonConvert.SerializeObject(document, Formatting.Indented, _jsonSettings)));
            return new IndexResponseModel { IsValid = response.IsValid };
        }

        private string GetIndexName(string indexName)
        {
            string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var index = $"{indexName.ToLowerInvariant()}_{DateTime.Now:yyyy.MM.dd}";

            if (!string.IsNullOrEmpty(env))
            {
                index = $"{env.Trim().ToLower()}_{index}";
            }
            return index.Replace(".", "_").Replace("-", "_");
        }

        private async Task<IndexResponseModel> CreateIndexCustom<T>(string indexName, CancellationToken ct = default) where T : class
        {
            string index = $"{indexName}_000001";
            var indexSettings = new IndexSettings
            {
                NumberOfReplicas = 1,
                NumberOfShards = 1
            };
            var response = await _elasticClient.Indices.CreateAsync(index, x =>
                                                                      x.RequestConfiguration(r => r.RequestTimeout(TimeSpan.FromMilliseconds(_pingTimeMilliSeconds)))
                                                                      .Map<T>(m => m.AutoMap())
                                                                      .InitializeUsing(new IndexState { Settings = indexSettings })
                                                                      .Aliases(a => a.Alias(indexName.ToString() + LogAlias))
                                                                      .Aliases(a => a.Alias(indexName, wr => wr.IsWriteIndex(true)))
                                                                      );
            return new IndexResponseModel
            {
                IsValid = response.IsValid,
            };

        }
    }

    public interface IElasticContext
    {
        Task<IndexResponseModel> IndexCustomAsync<T>(string indexName, T document, CancellationToken ct = default) where T : class;
        IndexResponseModel IndexCustom<T>(string indexName, T document) where T : class;
    }

    public class IndexResponseModel : IElasticsearchResponse
    {
        private bool _isValid = false;
        public bool IsValid { get { return ApiCall != null ? ApiCall.Success : _isValid; } set { _isValid = value; } }
        public string StatusMessage { get { return ApiCall != null ? ApiCall.ToString() : null; } }
        public Exception Exception { get { return ApiCall != null ? ApiCall.OriginalException : null; } }
        public IApiCallDetails ApiCall { get; set; }

        public bool TryGetServerErrorReason(out string reason)
        {
            reason = "";
            return false;
        }
    }

    public class ElasticSearchConfigModel
    {
        public string ConnectionString { get; set; }
        public int PingTimeMilliSeconds { get; set; }
    }

    public class LowercaseContractResolver : DefaultContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            return propertyName.ToLower();
        }
    }

    public class BaseLogModel
    {
        public BaseLogModel()
        {

        }
        public BaseLogModel(string message)
        {
            Message = message;
        }
        public string RequestID { get; set; }
        public DateTime RequestTime { get; set; }
        public DateTime ResponseTime { get; set; }
        public double Duration { get; set; }
        public string Message { get; set; }
    }
}

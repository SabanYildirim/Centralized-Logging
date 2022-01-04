using System;
using System.Collections.Generic;
using System.Text;

namespace CentralizedLogging.Core
{
    public class LogKeyConstants
    {
        public const string CorrelationId = "CorrelationId";
        public const string Version = "Version";
        public const string Payload = "RequestPayload";
        public const string Url = "RequestUrl";
        public const string QueryString = "RequestQuery";
        public const string HttpMethod = "HttpMethod";
        public const string RemoteIpAddress = "RemoteIpAddress";
        public const string Instance = "Instance";
        public const string Application = "ApplicationName";
        public const string DateTimeUTC = "DatetimeUTC";
        public const string RequestSequence = "RequestSequence";
        public const string PreviewRequestId = "PreviewRequestId";
        public const string RoutingKey = "QueueRoutingKey";
        public const string QueueName = "QueueName";
    }
}

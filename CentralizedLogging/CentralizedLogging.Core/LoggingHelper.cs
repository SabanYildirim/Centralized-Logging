using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.RabbitMQ;
using System;
using System.Collections.Generic;
using System.Text;

namespace CentralizedLogging.Core
{
    public static class LoggingHelper
    {
        public static Logger CustomLoggerConfiguration(CustomLoggerConfigurationModel model)
        {
            return new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithProperty(LogKeyConstants.Instance, model.InstanceId)
            .Enrich.WithProperty(LogKeyConstants.DateTimeUTC, DateTime.UtcNow)
            .Enrich.WithProperty(LogKeyConstants.Application, model.Application)
            .MinimumLevel.Override("Microsoft", model.MicrosoftLogLevel)
            .MinimumLevel.Override("System", model.SystemLogLevel)
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.RabbitMQ((clientConfiguration, sinkConfiguration) =>
            {
                clientConfiguration.Username = "guest";
                clientConfiguration.Password = "guest";
                clientConfiguration.Exchange = "LoggerQueue";
                clientConfiguration.ExchangeType = "fanout";
                clientConfiguration.DeliveryMode = RabbitMQDeliveryMode.Durable;
                clientConfiguration.Port = 5672;
                clientConfiguration.VHost = "/";
                clientConfiguration.Hostnames.Add("localhost");
                sinkConfiguration.RestrictedToMinimumLevel = model.DefaultLogLevel;
                sinkConfiguration.TextFormatter = new JsonFormatter();
            })
            .CreateLogger();
        }
    }

    public class CustomLoggerConfigurationModel
    {
        public CustomLoggerConfigurationModel()
        { }
        public CustomLoggerConfigurationModel(string application, string instanceId)
        {
            Application = application;
            InstanceId = instanceId;
        }
        public string Application { get; set; }
        public string InstanceId { get; set; }
        public LogEventLevel DefaultLogLevel { get; set; } = LogEventLevel.Information;
        public LogEventLevel MicrosoftLogLevel { get; set; } = LogEventLevel.Warning;
        public LogEventLevel SystemLogLevel { get; set; } = LogEventLevel.Warning;
    }
}

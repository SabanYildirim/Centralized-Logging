using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace CentralizedLogging.Core
{
    public static class RabbitMqExtension
    {
        public static IServiceCollection AddRabbitMq(this IServiceCollection services, RabbitMqConfigModel rabbitMqConfig)
        {
            services.AddCap(x =>
            {
                x.UseInMemoryStorage();
                x.UseRabbitMQ(opt =>
                {
                    opt.HostName = rabbitMqConfig.RabbitMqHostname;
                    opt.Password = rabbitMqConfig.RabbitMqPassword;
                    opt.UserName = rabbitMqConfig.RabbitMqUsername;
                    opt.Port = 5672;
                });
                x.ConsumerThreadCount = 2;
                x.FailedRetryCount = 5;

            });
            services.AddScoped<IRabbitMqService, RabbitMqService>();
            return services;
        }
    }
}

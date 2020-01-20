using Consul;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace ConsulExtensions
{
    public static class NextloadConsulServiceCollectionExtensions
    {
        /// <summary>
        /// Add Consul
        /// 添加consul
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddConsul(this IServiceCollection services, IConfiguration configuration)
        {
            // configuration Consul register address
            //配置consul注册地址
            services.Configure<ServiceDiscoveryOptions>(configuration.GetSection("ServiceDiscovery"));

            //configuration Consul client
            //配置consul客户端
            services.AddSingleton<IConsulClient>(sp => new Consul.ConsulClient(config =>
            {
                var consulOptions = sp.GetRequiredService<IOptions<ServiceDiscoveryOptions>>().Value;
                if (!string.IsNullOrWhiteSpace(consulOptions.Consul.HttpEndPoint))
                {
                    config.Address = new Uri(consulOptions.Consul.HttpEndPoint);
                    config.Datacenter = "dc1";
                }
            }));

            return services;
        }
    }
}
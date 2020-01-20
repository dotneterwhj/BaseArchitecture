using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace ConsulExtensions
{
    public static class NextloadConsulApplicationBuilderExtensions
    {
        /// <summary>
        /// use Consul
        /// 使用consul
        /// The default health check interface format is http://host:port/HealthCheck
        /// 默认的健康检查接口格式是 http://host:port/HealthCheck
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseConsul(this IApplicationBuilder app, /*IWebHostEnvironment env,*/int port)
        {
            IConsulClient consul = app.ApplicationServices.GetRequiredService<IConsulClient>();
            IApplicationLifetime appLife = app.ApplicationServices.GetRequiredService<IApplicationLifetime>();

            IOptions<ServiceDiscoveryOptions> serviceOptions =
                    app.ApplicationServices.GetRequiredService<IOptions<ServiceDiscoveryOptions>>();
            

            //FeatureCollection features = app.Properties["server.Features"] as FeatureCollection;
            //port = new Uri(server.Features.Get<IServerAddressesFeature>().Addresses.FirstOrDefault()).Port;
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"application port is :{port}");



            #region 注册本地Ip地址
            var addressIpv4Hosts = NetworkInterface.GetAllNetworkInterfaces()
                .OrderByDescending(c => c.Speed)
                .Where(c => c.NetworkInterfaceType != NetworkInterfaceType.Loopback && c.OperationalStatus == OperationalStatus.Up);
            foreach (NetworkInterface item in addressIpv4Hosts)
            {
                var props = item.GetIPProperties();
                //this is ip for ipv4
                //这是ipv4的ip地址
                string firstIpV4Address =
                    props.UnicastAddresses
                    .Where(c => c.Address.AddressFamily == AddressFamily.InterNetwork)
                    .Select(c => c.Address)
                    .FirstOrDefault().ToString();
                string serviceId = $"{serviceOptions.Value.ServiceName}_{firstIpV4Address}:{port}";
                AgentServiceCheck httpCheck = new AgentServiceCheck()
                {
                    DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(10),
                    Interval = TimeSpan.FromSeconds(5),
                    //this is default health check interface
                    //这个是默认健康检查接口
                    HTTP = $"{Uri.UriSchemeHttp}://{firstIpV4Address}:{port}/HealthCheck/",
                };
                AgentServiceRegistration registration = new AgentServiceRegistration()
                {
                    Checks = new[] { httpCheck },
                    Address = firstIpV4Address.ToString(),
                    ID = serviceId,
                    Name = serviceOptions.Value.ServiceName,
                    Port = port
                };
                consul.Agent.ServiceRegister(registration).GetAwaiter().GetResult();
                //send consul request after service stop
                //当服务停止后想consul发送的请求
                appLife.ApplicationStopping.Register(() =>
                {
                    consul.Agent.ServiceDeregister(serviceId).GetAwaiter().GetResult();
                });
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"health check service:{httpCheck.HTTP}");
            }
            #endregion

            #region 注册本地localhost地址
            //register localhost address
            //注册本地地址
            //AgentServiceRegistration localhostregistration = new AgentServiceRegistration()
            //{
            //    Checks = new[] {
            //        new AgentServiceCheck()
            //        {
            //            DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1),
            //            Interval = TimeSpan.FromSeconds(30),
            //            HTTP = $"{Uri.UriSchemeHttp}://localhost:{port}/HealthCheck", }
            //    },
            //    Address = "localhost",
            //    ID = $"{serviceOptions.Value.ServiceName}_localhost:{port}",
            //    Name = serviceOptions.Value.ServiceName,
            //    Port = port
            //};
            //consul.Agent.ServiceRegister(localhostregistration).GetAwaiter().GetResult();
            ////send consul request after service stop
            ////当服务停止后想consul发送的请求
            //appLife.ApplicationStopping.Register(() =>
            //{
            //    consul.Agent.ServiceDeregister(localhostregistration.ID).GetAwaiter().GetResult();
            //});
            #endregion

            app.Map("/HealthCheck", s =>
            {
                s.Run(async context =>
                {
                    await context.Response.WriteAsync("ok");
                });
            });
            return app;
        }
    }
}
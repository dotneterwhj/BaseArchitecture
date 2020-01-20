using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BaseArchitecture.WebUiProj.Models;
using BaseArchitecture.Frawework.Utility.WebapiHelper;
using Consul;

namespace BaseArchitecture.WebUiProj.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private static int _seedIndex = 0;
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult TestConsul()
        {
            #region 直接调用
            {
                //string url = "http://192.168.2.178:23997/WeatherForecast";
                //string result = WebapiHelperExtend.HttpRequestGet(url);
                //base.ViewBag.ApiResult = result;
                //base.ViewBag.ApiUrl = url;
            }
            #endregion

            #region 获取Consul注册的服务
            {
                //using (ConsulClient client = new ConsulClient(c =>
                //{
                //    c.Address = new Uri("http://192.168.2.188:8500/");
                //    c.Datacenter = "dc1";
                //}))
                //{
                //    var dictionary = client.Agent.Services().Result.Response;
                //    string message = "";
                //    foreach (var keyValuePair in dictionary)
                //    {
                //        AgentService agentService = keyValuePair.Value;
                //        this._logger.LogWarning($"{agentService.Address}:{agentService.Port} {agentService.ID} {agentService.Service}");//找的是全部服务 全部实例  其实可以通过ServiceName筛选
                //        message += $"{agentService.Address}:{agentService.Port};";
                //    }
                //    //获取当前consul的全部服务
                //    base.ViewBag.Message = message;
                //}
            }
            #endregion

            #region 调用服务

            //string url = "http://localhost:5726/api/users/get";
            //string url = "http://localhost:5727/api/users/get";
            //string url = "http://localhost:5728/api/users/get";
            string url = "http://WebapiServer/WeatherForecast";
            //consul解决使用服务名字 转换IP:Port----DNS

            Uri uri = new Uri(url);
            string groupName = uri.Host;
            using (ConsulClient client = new ConsulClient(c =>
            {
                c.Address = new Uri("http://192.168.2.188:8500");
                c.Datacenter = "dc1";
            }))
            {
                var dictionary = client.Agent.Services().Result.Response;
                var list = dictionary.Where(k => k.Value.Service.Equals(groupName, StringComparison.OrdinalIgnoreCase));//获取consul上全部对应服务实例
                KeyValuePair<string, AgentService> keyValuePair = new KeyValuePair<string, AgentService>();
                //拿到3个地址，只需要从中选择---可以在这里做负载均衡--
                //{
                //    keyValuePair = list.First();//直接拿的第一个
                //}
                {
                    var array = list.ToArray();
                    //随机策略---平均策略
                    keyValuePair = array[new Random(_seedIndex++).Next(0, array.Length)];
                }
                //{
                //    var array = list.ToArray();
                //    //轮询策略---平均策略
                //    keyValuePair = array[iSeed++ % array.Length];
                //}
                //{
                    ////权重---注册服务时指定权重，分配时获取权重并以此为依据
                    //List<KeyValuePair<string, AgentService>> pairsList = new List<KeyValuePair<string, AgentService>>();
                    //foreach (var pair in list)
                    //{
                    //    int count = int.Parse(pair.Value.Tags?[0]);
                    //    for (int i = 0; i < count; i++)
                    //    {
                    //        pairsList.Add(pair);
                    //    }
                    //}
                    //keyValuePair = pairsList.ToArray()[new Random(iSeed++).Next(0, pairsList.Count())];
                //}
                base.ViewBag.ApiUrl = $"{uri.Scheme}://{keyValuePair.Value.Address}:{keyValuePair.Value.Port}{uri.PathAndQuery}";
                base.ViewBag.ApiResult = WebapiHelperExtend.HttpRequestGet(base.ViewBag.ApiUrl);
            }

            #endregion
            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

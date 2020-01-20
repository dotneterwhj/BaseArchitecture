using System;
using System.Collections.Generic;
using System.Text;

namespace ConsulExtensions
{
    public class ServiceDiscoveryOptions
    {
        public string ServiceName { get; set; }

        public ConsulOptions Consul { get; set; }
    }

    public class ConsulOptions
    {
        public string HttpEndPoint { get; set; }
    }
}

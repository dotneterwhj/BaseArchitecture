using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace BaseArchitecture.Frawework.Utility.WebapiHelper
{
    public static class WebapiHelperExtend
    {
        public static string HttpRequestGet(string url)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                HttpRequestMessage message = new HttpRequestMessage();
                message.Method = HttpMethod.Get;
                message.RequestUri = new Uri(url);
                var result = httpClient.SendAsync(message).Result;
                string content = result.Content.ReadAsStringAsync().Result;
                return content;
            }
        }
    }
}

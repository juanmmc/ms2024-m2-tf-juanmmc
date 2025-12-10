using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogisticsAndDeliveries.IntegrationTest.Factories
{
    public class HttpClientFactory
    {
        public static HttpClient CreateClient()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:5234");
            return client;
        }
    }
}

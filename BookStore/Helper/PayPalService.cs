using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.Helper
{
    public class PayPalService
    {
        public static Dictionary<string,string> GetPayPalConfig()
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            var configuration = builder.Build();
            PayPalConfig paypal =  new PayPalConfig() {
                PayPalMode = configuration["PayPalMode"],
                PayPalConnectionTimeout = configuration["PayPalConnectionTimeout"],
                PayPalRequestEntries = configuration["PayPalRequestEntries"],
                PayPalClientId = configuration["PayPalClientId"],
                PayPalClientSecret = configuration["PayPalClientSecret"]
            };
            return paypal.getConfig();
        }
    }
}

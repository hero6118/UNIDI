using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace Core.Models.Request
{
    public class InfoCoinRequest
    {
        public class CryptoInfo
        {
            public string Symbol { get; set; }
            public decimal Price { get; set; }

        }

        public class CryptoApiResponse
        {
            [JsonProperty("data")]
            public Dictionary<string, CryptoData> Data { get; set; }
        }

        public class CryptoData
        {
            [JsonProperty("quote")]
            public QuoteData Quote { get; set; }
        }

        public class QuoteData
        {
            [JsonProperty("USD")]
            public UsdData USD { get; set; }
        }

        public class UsdData
        {
                [JsonProperty("price")]
             public decimal Price { get; set; }
                [JsonProperty("percent_change_24h")]
                public double Percent_change_24h { get; set; }  
        }
    }
}

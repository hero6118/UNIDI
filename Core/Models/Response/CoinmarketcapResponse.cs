using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.Response
{
    public class CoinmarketcapResponse
    {
        public List<CoinmarketcapData> Data { get; set; }
        public CoinmarketcapStatus Status { get; set; }
    }

    public class CoinmarketcapData
    {
        public string Symbol { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public double Amount { get; set; }
        public DateTime Last_updated { get; set; }
        public CoinmarketcapQuote Quote { get; set; }
    }

    public class CoinmarketcapQuote
    {
        public CoinmarketcapUSD USD { get; set; }
    }


    public class CoinmarketcapStatus
    {
        public DateTime Timestamp { get; set; }
        public int Error_code { get; set; }
        public string Error_message { get; set; }
        public int Elapsed { get; set; }
        public int Credit_count { get; set; }
        public string Notice { get; set; }
    }

    public class CoinmarketcapUSD
    {
        public double Price { get; set; }
        
        public DateTime Last_updated { get; set; }
    }

}

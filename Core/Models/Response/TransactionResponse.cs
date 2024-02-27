using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.Response
{
    public class TransactionResponse
    {
        public bool Check { get; set; }
        public string Ms { get; set; }
        public List<Transaction> Data { get; set; }
        public long Total { get; set; }
    }
    public class BalanceResponse
    {
        public bool Check { get; set; }
        public string Ms { get; set; }
        public double Data { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.Request
{
    public class WalletRequest
    {
        public Guid TokenId { get; set; }   
       
        public double Amount { get; set; }
        public string type { get; set; }
        public string TransactionFrom { get; set; }

        public int? Chain { get; set; }
 
    }
}

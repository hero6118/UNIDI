using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.Request
{
    public class CreateVoucherRequest
    {
        public Guid? IdVoucher { get; set; }
        public string NameVoucher { get; set; }
        public double? PriceDiscount { get; set; } 
        public int? Count { get; set; } 
        public string Code { get; set; }    
    }
}

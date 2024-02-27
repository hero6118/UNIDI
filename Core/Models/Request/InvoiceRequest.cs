using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.Request
{
    public class OrderListShop
    {
        public List<string> ShopId { get; set; }
        public List<Guid> LogisticId { get; set; }
        public List<string> CodeVoucher { get; set; }
        public string Note { get; set; }
        public string AddressId { get; set; }
        public string Currency { get; set; }
    }

    public class InvoiceRequest
        {
        public List<Guid> InvoiceId { get; set; }
        public int? Statusinv { get; set; }
    }

}

   

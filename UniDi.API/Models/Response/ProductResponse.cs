using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIService.Models.Response
{
    public class ProductResponse
    {
        public bool Status { get; set; }
        public string? Message { get; set; }
        public List<ProductResult>? Result { get; set; }
    }
    public class ProductResult
    {
        public Guid ID { get; set; }
        public string? BrandName { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public double OldPrice { get; set; }
        public double NewPrice { get; set; }
        public double DiscountPercent { get; set; }
        public string? Image { get; set; }
    }
}

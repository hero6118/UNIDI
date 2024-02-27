using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIService.Models.Response
{
    public class BrandNameResponse
    {
        public bool Status { get; set; }
        public string? Message { get; set; }
        public List<BrandNameResult>? Result { get; set; }
    }
    public class BrandNameResult
    {
        public string? Name { get; set; }
        public Guid ID { get; set; }
    }

}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace APIService.Models.Request
{
    public class ProductListRequest
    {
        [DefaultValue("1")]
        public int Page { get; set; }
        [DefaultValue("20")]
        public int Limit { get; set; }
        [DefaultValue("asc")]
        public string? Order { get; set; }
        public string? FilterType { get; set; }
        public string? FilterKeyword { get; set; }
    }
}

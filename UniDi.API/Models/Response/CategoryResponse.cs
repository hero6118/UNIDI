using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIService.Models.Response
{
    public class CategoryResponse
    {
        public bool Status { get; set; }
        public string? Message { get; set; }
        public List<CategoryResult>? Result { get; set; }
    }
    public class CategoryResult
    {
        public string? Name { get; set; }
        public Guid ID { get; set; }
    }
}

using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.Request
{
    public class LicenseRequest
    {
       
         
        public string Name { get; set; }
       
        public string Description { get; set; }

        public string BusinessCode { get; set; }
        public string Role { get; set; }
        public IFormFile LicenseImage { get; set; }
        public IFormFile Logo { get; set; }
        public IFormFile DistributionAgentContractImage { get; set; }
    }
    public class ProductLicenseRequest
    {
        public string Name { get; set; }
      
        public IFormFile LicenseImage { get; set; }
        public IFormFile ProductImage { get; set; }      
    }

}

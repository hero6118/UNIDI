using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.Request
{
    public class SellRequest
    {
    }
    public class LogisticRequest
    {
        public Guid? Id { get; set; }
        public string LogisticName { get; set; }

        public float? MaxWeight { get; set; }    
        public float? ShipPrice { get; set; }    
        public float? StepPrice { get; set; }
        public float? StepWeight { get; set; }   
        public IFormFile Logo { get; set; } 

    }
}

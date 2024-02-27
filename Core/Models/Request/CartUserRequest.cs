using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.Request
{
   
    public class CartRequest
    {
        public int? IdCart { get; set; }
        public Guid? ProductId { get; set; }    
        public int? Quantity { get; set; }
        public string Type { get; set; }    
        public string TokenName { get; set; }  
        public Guid? PropertyId { get; set; }
    } 
}

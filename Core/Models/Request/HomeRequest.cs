using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.Request
{
    public class HomeRequest
    {
    }

    public class AddressRequest
    {
        public string Id { get; set; }
        public string NameReCevive { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string ZipCode { get; set; }
        public string Phone { get; set; }
        public string DetailAddress { get; set; }
        public string Country { get; set; }

        public string Type { get; set; }
    }
    public class FavoriteRequest
    {
        public Guid?  Id { get; set; }  
        public Guid? ProductId { get;set; }
    }
}

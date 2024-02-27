using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.Request
{
    public class ListCoinRequest
    {
        public string Id { get; set; }
        [Required]
        public string EmailUser { get; set; }
        public string RelationshipProject { get; set; }
        public Nullable<System.DateTime> DateLaunch { get; set; }
        public string ProjectName { get; set; }
        public string Symbol { get; set; }
        public string DetailProjectDescription { get; set; }
        public string Platform { get; set; }
        public string MediaCoverage { get; set; }
        public string Country { get; set; }
        public string Logo { get; set; }
        public string Website1 { get; set; }
        public string Website2 { get; set; }
        public string PlatformOfContract { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string BlockExplorer { get; set; }
        public string Affiliation { get; set; }
        public string Whitepaper { get; set; }
        public string Twitter { get; set; }
        public string Telegram { get; set; }
        public string Reddit { get; set; }
        public string Facebook { get; set; }
        public string ProjectVideo { get; set; }
        public string Linkedln { get; set; }
        public string Status { get; set; }
        public string PriceLink { get; set; }
        
        public string Contract { get; set; }    
        public string ChainName { get; set; }   
        public int? Decimals { get ; set; } 

    }
}

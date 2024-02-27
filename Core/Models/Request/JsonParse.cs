using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.Request
{
    public class JsonParse
    {
        public class DataModel
        {
            public string PrivateKey { get; set; }
            public string Address { get; set; } 
            // Other properties as needed
        }

        public class JsonResponseCreateWallet
        {
            public bool status { get; set; }
            public string message { get; set; }
            public DataModel result { get; set; }
        }

        public class JsonResponseCheckAddress
        {
            public bool status { get; set; }
            public string message { get; set; }
            public DataContract result { get; set; }
        }

        public class DataContract
        {
            public string Contract { get; set; }
            public string Name { get; set; }
            public string Symbol { get; set; }
            public string Logo { get; set; }
            public int? Decimals { get; set; }
        }
    }
}
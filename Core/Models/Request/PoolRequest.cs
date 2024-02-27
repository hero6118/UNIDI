using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.Request
{
    public class PoolRequest
    {

    }

    public class TypeCryptoRequest
    {
        public string Id { get; set; }  
        public string Name { get; set; }
        public string Code { get; set; }
        public IFormFile Image { get; set; }
    }
    public class CreatePoolRequest
    {
        public Guid? Id { get; set; }
        public double? Price { get; set; }
        public bool? IsPriceRealTime { get; set; }
        public double? USDAmount { get; set; }
        public double? PercentAcceptUSDPay { get; set; }
        public string Currency { get; set; }
    }


    public  class AddPoolProductRequest
    {
        public Guid PoolId { get; set; }
        public bool? AcceptAllCountry { get; set; }
        public List<int?> ListCountryId { get; set; }
        public bool? AcceptAllCategory { get; set; }
        public List<string> ListCategoryId { get; set; }
        public bool? AcceptAllShop { get; set; }
        public List<string> ListShopId { get; set; }
        public bool? AcceptAllProduct { get; set; }
        public List<string> ListProductId { get; set; }
    }
    public class DeletePoolItemRequest
    {
        public Guid PoolId { get; set; }
        public string Type { get; set; }
        public int? IntId { get; set; }
        public string StringId { get; set; }
    }
    public class AddProductRequest
    {
        public string IdProduct { get; set; }
        public string IdPool { get; set; }
       
    }

    
    public class DepositRequest
    {
        public double? depositss { get; set;}
        
        public string MethodPayment {get;set;}
    }
   
    public class AddDetailRequest
    { 
        public string Id { get; set; }
        public string Service { get; set; }
        public string Details { get; set; }
    }

}

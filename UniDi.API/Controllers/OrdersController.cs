using Core;
using Core.Models.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenCvSharp;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Security.Claims;
using UniDi.API.Models;
using X.PagedList;
using static OpenCvSharp.Stitcher;

namespace UniDi.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        /* [HttpGet("[action]")]
         public ActionResult GetOrdersBySeller(int page = 1, int limit = 20)
         {

             var user = C_User.Auth(Request.Headers["Authorization"]);
             if (user == null)
                 return Ok(new { check = false, ms = "Your login session has expired, please login again!" });

             //return Ok(new { check = false, user });
             using (var de = new DataEntities())
             {
                 de.Configuration.LazyLoadingEnabled = false; // 1 cai doc 1 cai ghi
                 de.Configuration.ProxyCreationEnabled = false;

                 //var getcate = from r in de.Categories select r;
                 var list = de.Orders.AsNoTracking().Where(p => p.Product.UserId == user.Id).OrderByDescending(p => p.DateCreate).ToPagedList(page, limit);



                 foreach (var item in list)
                 {
                     item.Product = de.Products.AsNoTracking().FirstOrDefault(p => p.Id == item.ProductId);
                     item.AspNetUser = de.AspNetUsers.AsNoTracking().FirstOrDefault(p => p.Id == item.UserBuyID);
                 }
                 return Ok(new { check = true, ms = "success!", data = list, total = list.TotalItemCount });
             }
             // Console.WriteLine("text");

         }
 */
        [HttpGet("[action]")]
        [SwaggerOperation( Description ="If <b>type </b>!= null get all by userID \n\n| get all by shopId statusinv from 0-3")]
        public ActionResult GetInvoiceById( DateTime? from,DateTime? to,string? searchcode,int? statusinv,string? type ,int page = 1, int limit = 20)
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });


            using var de = new DataEntities();
            de.Configuration.LazyLoadingEnabled = false; // 1 cai doc 1 cai ghi
            de.Configuration.ProxyCreationEnabled = false;
            
            if (to != null)
                to = to.Value.Add(new TimeSpan(23, 59, 59));

            //var getcate = from r in de.Categories select r;
            var list = de.Invoices.AsNoTracking().Where(p =>
       ((string.IsNullOrEmpty(type) && p.ShopId == user.Id)
            || (!string.IsNullOrEmpty(type) && p.UserId == user.Id))
            && (statusinv == null || p.DeliveryStatus == statusinv)
            &&(string.IsNullOrEmpty(searchcode) || p.Code.Contains(searchcode))
            && (from == null || p.DateCreate >= from)
            && (to == null || p.DateCreate  <= to)  
            ).OrderByDescending(p => p.DateCreate).ToPagedList(page, limit);

            foreach (var item in list)
            {
                item.User = de.AspNetUsers.AsNoTracking().FirstOrDefault(p => p.Id == item.UserId);
                item.InvoiceDetails = de.InvoiceDetails.AsNoTracking().Where(p => p.InvoiceId == item.Id).ToList();
                foreach (var detail in item.InvoiceDetails)
                {
                    detail.Product = de.Products.AsNoTracking().FirstOrDefault(p => p.Id == detail.ProductId);
                    var propertyproduct = de.ProductProperties.AsNoTracking().Any(p => p.ProductId == detail.ProductId);
                    if (propertyproduct && detail.Product != null)
                        detail.Product.ProductProperty = de.ProductProperties.AsNoTracking().Where(p => p.ProductId == detail.ProductId).ToList();
                }
            }

            return Ok(new { check = true, ms = "success!", data = list, total = list.TotalItemCount });
        }
      
        [HttpPost("[action]")]
        [SwaggerOperation (Description = "update all from Package to Delivering")]
        public ActionResult UpdateAllStatusInvoice()
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });
            using (var de = new DataEntities())
            {
                 var invoices = de.Invoices.Where(p => p.ShopId == user.Id &&  p.DeliveryStatus == Enum_DeliveryStatus.Packaged);
                //       var list = de.Invoices.Where(p => p.ShopId == user.Id).OrderByDescending(p => p.DateCreate).ToPagedList(page, limit); 
                foreach (var item in invoices)
                {
                    item.DeliveryStatus = Enum_DeliveryStatus.Delivering;
                    de.SaveChanges();
                }
                return Ok(new { check = true, ms = "Update all Status success !!!" });


            }
        }

        [HttpPost("[action]")]
        [SwaggerOperation(Description = "status shipping 0--7")]
        public ActionResult UpdateStatusInvoice([FromForm] InvoiceRequest model)
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!" });
            if (C_User.CheckAccept("Seller", user.Id) != true)
                return Ok(new {check =false, ms = "You do not have access" });
            

            using var de = new DataEntities();
            foreach (var item in model.InvoiceId)
            {
                var invoice = de.Invoices.FirstOrDefault(p => p.Id == item && p.ShopId == user.Id);
                if (invoice == null)
                    return Ok(new { check = false, ms = "Can't found this invoice" });
                invoice.DeliveryStatus = model.Statusinv;
                invoice.DateUpdate = DateTime.Now;
            } 
            
        
            return Ok(new { check = true, ms = "Update Status success !!!" });
        }

      /*  [HttpGet("[action]")]
        public ActionResult StatictialBy*/
    }   
}
    


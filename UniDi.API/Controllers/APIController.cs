using Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Newtonsoft.Json;

namespace UniDi.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class APIController : ControllerBase
    {
        [HttpGet("[action]")]
        public ActionResult GetJS()
        {
            var content = new
            {
                Enum_PaidStatus = Enum_PaidStatus.Label,
                Enum_UserType = Enum_UserType.Label,
                Enum_TransactionType = Enum_TransactionType.Label,
                Enum_ListCoinStatus = Enum_ListCoinStatus.Label,
                Enum_Units = C_Config.Units,
                Enum_productstatusACP = Enum_ProductStatus.label,
                Enum_productstatusStock = Enum_ProductStatusStock.Label,
                Enum_BusinessLicense = Enum_BusinessLicense.Label,
                Enum_DeliveryStatus = Enum_DeliveryStatus.Label,
            };

            return new ContentResult
            {
                Content = JsonConvert.SerializeObject(content),
                ContentType = "application/javascript",
                StatusCode = (int)HttpStatusCode.OK
            };
        }
        [HttpGet("[action]")]
        public void FixProductNote()
        {
            using var de = new DataEntities();
            var list = de.Products.OrderBy(p => p.Name).ToList();
            foreach (var item in list)
            {
                item.CateNode = de.Categories.AsNoTracking().FirstOrDefault(p => p.Id == item.CategoryId)?.CateNode;
            }
            de.SaveChanges();
        }
        //[HttpGet("[action]")]
        //public void FixCateNote()
        //{
        //    using (var de = new DataEntities())
        //    {
        //        var list = de.Categories.Where(p => p.ParentId == null).OrderBy(p => p.Name).ToList();
        //        for (int i = 0; i < list.Count; i++)
        //        {
        //            var item = list[i];
        //            item.CateNode = i + "";
        //            Loop_FixCateNote(item);
        //        }
        //        de.SaveChanges();
        //    }
        //}
        //[NonAction]
        //public void Loop_FixCateNote(Category cate)
        //{
        //    using (var de = new DataEntities())
        //    {
        //        var list = de.Categories.Where(p => p.ParentId == cate.Id).OrderBy(p => p.Name).ToList();
        //        for (int i = 0; i < list.Count; i++)
        //        {
        //            var item = list[i];
        //            item.CateNode = cate.CateNode + "-" + i;
        //            Loop_FixCateNote(item);
        //        }
        //        de.SaveChanges();
        //    }
        //}

    }
}
